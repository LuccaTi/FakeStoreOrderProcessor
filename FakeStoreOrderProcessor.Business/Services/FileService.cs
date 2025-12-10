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
        private string? ordersFolder;
        private string? processedFilesFolder;
        private string? cancelledOrdersFolder;
        private string? invalidFilesFolder;

        public FileService(ILogger<FileService> logger, IOptions<ServiceSettings> settings)
        {
            _logger = logger;
            _settings = settings;

            ordersFolder = _settings.Value.OrdersFolder;
            processedFilesFolder = _settings.Value.ProcessedFilesFolder;
            cancelledOrdersFolder = _settings.Value.CancelledOrdersFolder;
            invalidFilesFolder = _settings.Value.InvalidFilesFolder;

            ValidateAndCreateDirectories();
        }

        public List<string> GetAllFiles()
        {
            var currentMonthOrdersFolder = Path.Combine(ordersFolder!, DateTime.Now.ToString("yyyy/MM"));
            if (!Directory.Exists(currentMonthOrdersFolder))
                return new List<string>();

            var files = Directory.EnumerateFiles(currentMonthOrdersFolder, "*.*", SearchOption.AllDirectories)
                .OrderBy(file => file)
                .ToList();

            return files;
        }

        public void ValidateAndCreateDirectories()
        {
            if (string.IsNullOrEmpty(ordersFolder))
                throw new Exception("Orders folder was not provided!");

            if (string.IsNullOrEmpty(processedFilesFolder))
                throw new Exception("Processed files folder was not provided!");

            if (string.IsNullOrEmpty(cancelledOrdersFolder))
                throw new Exception("Cancelled orders folder was not provided");

            if (string.IsNullOrEmpty(invalidFilesFolder))
                throw new Exception("Invalid files folder was not provided!");

            if (!Directory.Exists(ordersFolder))
            {
                Directory.CreateDirectory(ordersFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {ordersFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Orders folder already exists");
            }

            if (!Directory.Exists(processedFilesFolder))
            {
                Directory.CreateDirectory(processedFilesFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {processedFilesFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Processed files folder already exists");
            }

            if (!Directory.Exists(cancelledOrdersFolder))
            {
                Directory.CreateDirectory(cancelledOrdersFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {cancelledOrdersFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Cancelled orders folder already exists");
            }

            if (!Directory.Exists(invalidFilesFolder))
            {
                Directory.CreateDirectory(invalidFilesFolder);
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - Folder: {invalidFilesFolder} created");
            }
            else
            {
                _logger.LogDebug($"{_className} - ValidateAndCreateDirectories - InvalidFiles folder already exists");
            }
        }

        public void MoveProcessedFile(string file)
        {
            string fileGuid = GetGuidFromOrderFile(file);
            string processedFolder = Path.Combine(processedFilesFolder!, DateTime.Now.ToString("yyyy/MM/dd"), fileGuid).Replace(@"/", "\\");

            if (!Directory.Exists(processedFolder))
                Directory.CreateDirectory(processedFolder);

            string destFile = Path.Combine(processedFolder, Path.GetFileName(file));
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
            string cancelledOrderFolder = Path.Combine(cancelledOrdersFolder!, DateTime.Now.ToString("yyyy/MM/dd")).Replace(@"/", "\\");

            if (!Directory.Exists(cancelledOrderFolder))
                Directory.CreateDirectory(cancelledOrderFolder);

            string destFile = Path.Combine(cancelledOrderFolder, Path.GetFileName(file));
            if (!File.Exists(destFile))
            {
                File.Move(file, destFile);
                _logger.LogDebug($"{_className} - MoveCancelledOrder - File moved: {destFile}");
            }
            else
            {
                _logger.LogWarning($"{_className} - MoveCancelledOrder - File: {destFile} already exists, copy will be sent to invalid files folder!");
                MoveInvalidFile(file);
            }
        }

        public void MoveInvalidFile(string file)
        {
            string invalidFileFolder = Path.Combine(invalidFilesFolder!, DateTime.Now.ToString("yyyy/MM/dd")).Replace(@"/", "\\");

            try
            {
                string fileGuid = GetGuidFromOrderFile(file);
                invalidFileFolder = Path.Combine(invalidFileFolder, fileGuid);
            }
            catch (InvalidFileException)
            {
                _logger.LogDebug($"{_className} - MoveInvalidFile - Could not obtain order guid from file: {file} - Invalid folder will have 'InvalidName' instead as folder name");
                invalidFileFolder = Path.Combine(invalidFileFolder, "InvalidName");
            }

            if (!Directory.Exists(invalidFileFolder))
                Directory.CreateDirectory(invalidFileFolder);

            string destFile = Path.Combine(invalidFileFolder, Path.GetFileName(file));
            if (!File.Exists(destFile))
            {
                File.Move(file, destFile);
                _logger.LogDebug($"{_className} - MoveInvalidFile - File moved: {destFile}");
            }
            else
            {
                string copyFileName = Path.GetFileName(destFile) + DateTime.Now.Ticks + "_copy";
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
                throw new InvalidFileException($"Could not obtain order guid from file: {file}");
            }
        }

    }
}
