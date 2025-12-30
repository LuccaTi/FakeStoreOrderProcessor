using FakeStoreOrderProcessor.Business.Exceptions;
using FakeStoreOrderProcessor.Business.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Services
{
    public class FileService : IFileService
    {
        private const string _className = "FileService";
        private readonly ILogger<FileService> _logger;
        private readonly IOptions<ServiceSettings> _settings;
        private string? _ordersFolder;
        public string? ProcessedFilesFolder { get; set; }
        private string? _processingFilesFolder;
        private string? _cancelledOrdersFolder;
        private string? _invalidFilesFolder;
        private string? _registeredProductsFolder;

        public FileService(ILogger<FileService> logger, IOptions<ServiceSettings> settings)
        {
            _logger = logger;
            _settings = settings;

            _ordersFolder = _settings.Value.OrdersFolder;
            ProcessedFilesFolder = _settings.Value.ProcessedFilesFolder;
            _processingFilesFolder = _settings.Value.ProcessingFilesFolder;
            _cancelledOrdersFolder = _settings.Value.CancelledOrdersFolder;
            _invalidFilesFolder = _settings.Value.InvalidFilesFolder;
            _registeredProductsFolder = _settings.Value.RegisteredProductsFolder;

            ValidateAndCreateDirectories();
        }

        public List<string> GetAllFiles()
        {
            var currentMonthOrdersFolder = Path.Combine(_ordersFolder!, DateTime.Now.ToString("yyyy/MM"));
            if (!Directory.Exists(currentMonthOrdersFolder))
                return new List<string>();

            var files = Directory.EnumerateFiles(currentMonthOrdersFolder, "*.*", SearchOption.AllDirectories)
                .OrderBy(file => file)
                .ToList();

            return files;
        }

        public List<string> GetAllProcessingFiles()
        {
            var files = Directory.EnumerateFiles(_processingFilesFolder!, "*.*", SearchOption.AllDirectories)
                .OrderBy(file => file)
                .ToList();

            return files;
        }

        public void ValidateAndCreateDirectories()
        {
            if (string.IsNullOrEmpty(_ordersFolder))
                throw new Exception("Orders folder path was not provided!");

            if (string.IsNullOrEmpty(ProcessedFilesFolder))
                throw new Exception("Processed files folder path was not provided!");

            if (string.IsNullOrEmpty(_cancelledOrdersFolder))
                throw new Exception("Cancelled orders folder path was not provided");

            if (string.IsNullOrEmpty(_invalidFilesFolder))
                throw new Exception("Invalid files folder path was not provided!");

            if (string.IsNullOrEmpty(_registeredProductsFolder))
                throw new Exception("Registered products folder path was not provided!");

            if (string.IsNullOrEmpty(_processingFilesFolder))
                throw new Exception("Processing files folder path was not provided!");

            if (!Directory.Exists(_ordersFolder))
            {
                Directory.CreateDirectory(_ordersFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {_ordersFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Orders folder already exists");
            }

            if (!Directory.Exists(ProcessedFilesFolder))
            {
                Directory.CreateDirectory(ProcessedFilesFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {ProcessedFilesFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Processed files folder already exists");
            }

            if (!Directory.Exists(_cancelledOrdersFolder))
            {
                Directory.CreateDirectory(_cancelledOrdersFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {_cancelledOrdersFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Cancelled orders folder already exists");
            }

            if (!Directory.Exists(_invalidFilesFolder))
            {
                Directory.CreateDirectory(_invalidFilesFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {_invalidFilesFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Invalid files folder already exists");
            }

            if (!Directory.Exists(_registeredProductsFolder))
            {
                Directory.CreateDirectory(_registeredProductsFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {_registeredProductsFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Registered products folder already exists");
            }

            if (!Directory.Exists(_processingFilesFolder))
            {
                Directory.CreateDirectory(_processingFilesFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {_processingFilesFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Processing files folder already exists");
            }
        }

        public void MoveProcessedFile(string file)
        {
            string fileName = Path.GetFileName(file);
            string fileGuid = GetGuidFromOrderFile(file);
            string? processedFolder = Path.Combine(ProcessedFilesFolder!, DateTime.Now.ToString("yyyy/MM/dd"), fileGuid).Replace(@"/", "\\");

            if(fileName.Contains("payment") || 
                fileName.Contains("shipped")||
                fileName.Contains("delivered"))
            {
                string searchFolder = Path.Combine(ProcessedFilesFolder!, DateTime.Now.ToString("yyyy/MM")).Replace(@"/", "\\");
                processedFolder = Directory.EnumerateDirectories(searchFolder, "*.*", SearchOption.AllDirectories)
                    .Where(dir =>  dir.Contains(fileGuid)).FirstOrDefault();
                if(string.IsNullOrEmpty(processedFolder))
                    processedFolder = Path.Combine(ProcessedFilesFolder!, DateTime.Now.ToString("yyyy/MM/dd"), fileGuid).Replace(@"/", "\\");
            }

            if (!Directory.Exists(processedFolder))
                Directory.CreateDirectory(processedFolder!);

            string destFile = Path.Combine(processedFolder!, Path.GetFileName(file));
            if (!File.Exists(destFile))
            {
                File.Move(file, destFile);
                _logger.LogDebug($"{_className} - MoveProcessedFile - File moved: {destFile}");
            }
            else
            {
                _logger.LogWarning($"{_className} - MoveProcessedFile - File: {destFile} already exists, copy will be sent to invalid files folder!");
                MoveInvalidFile(file);
            }
        }

        public void MoveCancelledOrder(string file)
        {
            string fileGuid = GetGuidFromOrderFile(file);
            string sourceDir = Path.GetDirectoryName(file)!;
            string directoryName = sourceDir.Split(Path.DirectorySeparatorChar).Last();
            string dayOfOrder = Path.GetDirectoryName(sourceDir)!.Split(Path.DirectorySeparatorChar).Last();
            string cancelledOrderFolder = Path.Combine(_cancelledOrdersFolder!, DateTime.Now.ToString("yyyy/MM"), dayOfOrder).Replace(@"/", "\\");

            if (!Directory.Exists(cancelledOrderFolder))
                Directory.CreateDirectory(cancelledOrderFolder);

            string destDir = Path.Combine(cancelledOrderFolder, directoryName);
            if (!Directory.Exists(destDir))
            {
                Directory.Move(sourceDir, destDir);
                _logger.LogDebug($"{_className} - MoveCancelledOrder - Directory moved: {destDir}");
            }
            else
            {
                _logger.LogWarning($"{_className} - MoveCancelledOrder - Directory: {destDir} already exists, copy will be sent to invalid files folder!");
                MoveInvalidFolder(sourceDir);
            }
        }

        public void MoveInvalidFile(string file)
        {
            string invalidFileFolder = Path.Combine(_invalidFilesFolder!, DateTime.Now.ToString("yyyy/MM/dd")).Replace(@"/", "\\");
            string fileName = Path.GetFileName(file);

            try
            {
                string fileGuid = GetGuidFromOrderFile(file);
                invalidFileFolder = Path.Combine(invalidFileFolder, fileGuid);
            }
            catch (InvalidFileException)
            {
                if (fileName.Contains("product"))
                {
                    _logger.LogDebug($"{_className} - MoveInvalidFile - Could not obtain order guid from product file: {Path.GetFileName(file)} - Invalid folder will have 'InvalidProductFiles' instead as folder name");
                    invalidFileFolder = Path.Combine(invalidFileFolder, "InvalidProductFiles");
                }
                else
                {
                    _logger.LogDebug($"{_className} - MoveInvalidFile - Could not obtain order guid from file: {Path.GetFileName(file)} - Invalid folder will have 'InvalidNameFiles' instead as folder name");
                    invalidFileFolder = Path.Combine(invalidFileFolder, "InvalidNameFiles");
                }
            }

            if (!Directory.Exists(invalidFileFolder))
                Directory.CreateDirectory(invalidFileFolder);

            string destFile = Path.Combine(invalidFileFolder, fileName);
            if (!File.Exists(destFile))
            {
                File.Move(file, destFile);
                _logger.LogDebug($"{_className} - MoveInvalidFile - File moved: {destFile}");
            }
            else
            {
                string copyFileName = DateTime.Now.Ticks + "_copy_" + fileName;
                destFile = Path.Combine(invalidFileFolder, copyFileName);

                try
                {
                    File.Move(file, destFile);
                    _logger.LogDebug($"{_className} - MoveInvalidFile - File moved: {destFile}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{_className} - MoveInvalidFile - Could not move invalid file: {destFile} - Error: {ex.Message} - Manual verification is required!");
                }
            }
        }

        public void MoveInvalidFolder(string folder)
        {
            string invalidFileFolder = Path.Combine(_invalidFilesFolder!, DateTime.Now.ToString("yyyy/MM/dd")).Replace(@"/", "\\");

            if (!Directory.Exists(invalidFileFolder))
                Directory.CreateDirectory(invalidFileFolder);

            string folderName = folder.Split(Path.DirectorySeparatorChar).Last();
            string destDir = Path.Combine(invalidFileFolder, folderName);

            if (!Directory.Exists(destDir))
            {
                Directory.Move(folder, destDir);
                _logger.LogDebug($"{_className} - MoveInvalidFolder - Directory moved: {destDir}");
            }
            else
            {
                folderName = DateTime.Now.Ticks + "_copy_" + folderName;
                destDir = Path.Combine(invalidFileFolder, folderName);
                try
                {
                    Directory.Move(folder, destDir);
                    _logger.LogDebug($"{_className} - MoveInvalidFolder - Directory moved: {destDir}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{_className} - MoveInvalidFolder - Could not move invalid directory: {destDir} - Error: {ex.Message} - Manual verification is required!");
                }
            }
        }

        public void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
                _logger.LogDebug($"{_className} - DeleteFile - File deleted: {file}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_className} - DeleteFile - Could not delete file: {file} - Error: {ex.Message} - Manual verification is required!");
            }
        }

        public void MoveRegisteredProduct(string file)
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(_registeredProductsFolder!, fileName);

            if (!File.Exists(destFile))
            {
                File.Move(file, destFile);
                _logger.LogDebug($"{_className} - MoveRegisteredProduct - File moved: {destFile}");
            }
            else
            {
                throw new InvalidFileException("File already exists in registered products folder!");
            }
        }

        public string MoveToProcessing(string file)
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(_processingFilesFolder!, fileName);

            if (!File.Exists(destFile))
            {
                File.Move(file, destFile);
                _logger.LogDebug($"{_className} - MoveToProcessing - File moved: {destFile}");
                return destFile;
            }
            else
            {
                throw new InvalidFileException("File already exists in processing files folder!");
            }
        }

        public string GetGuidFromOrderFile(string file)
        {
            var regex = new Regex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

            string fileName = Path.GetFileName(file);
            int start = fileName.IndexOf('_') + 1;
            int end = fileName.IndexOf('_', start);
            int length = end - start;
            string fileGuid = fileName.Substring(start, length);

            var match = regex.Match(fileGuid);
            if (match.Success)
            {
                return fileGuid;
            }
            else
            {
                throw new InvalidFileException($"Could not obtain order guid from file: {fileName}");
            }
        }

    }
}
