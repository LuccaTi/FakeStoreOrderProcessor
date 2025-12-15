using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using FakeStoreOrderProcessor.Business.Engines.Interfaces;
using FakeStoreOrderProcessor.Business.Exceptions;
using FakeStoreOrderProcessor.Business.Services.Interfaces;
using FakeStoreOrderProcessor.Library.DTO.Json;
using FakeStoreOrderProcessor.Library.DTO.Product;
using FakeStoreOrderProcessor.Library.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FakeStoreOrderProcessor.Business.Engines
{
    public class ServiceEngine : IServiceEngine
    {
        private const string _className = "ServiceEngine";
        private readonly ILogger<ServiceEngine> _logger;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IApiService _apiService;

        public ServiceEngine(ILogger<ServiceEngine> logger, IMapper mapper, IFileService fileService, IApiService apiService)
        {
            _logger = logger;
            _mapper = mapper;
            _fileService = fileService;
            _apiService = apiService;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{_className} - ProcessAsync - Starting processing");
            var processingFiles = _fileService.GetAllProcessingFiles();
            var files = _fileService.GetAllFiles();

            if (processingFiles.Any())
            {
                var processingProductFiles = processingFiles.Where(file => Path.GetFileName(file).Contains("product")).ToList();
                foreach (var file in processingProductFiles)
                {
                    await ProcessSingleProductFile(file, false, cancellationToken);
                }
            }

            var productFiles = files.Where(file => Path.GetFileName(file).Contains("product")).ToList();
            foreach (var file in productFiles)
            {
                await ProcessSingleProductFile(file, true, cancellationToken);
            }
        }

        public async Task ProcessSingleProductFile(string file, bool moveToProcessing, CancellationToken cancellationToken)
        {

            string currentFile = file;
            string fileName = Path.GetFileName(file);
            _logger.LogDebug($"{_className} - ProcessSingleProductFile - Processing file: {fileName}");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (moveToProcessing)
                    currentFile = _fileService.MoveToProcessing(file);

                string jsonContent = await File.ReadAllTextAsync(currentFile, cancellationToken);
                var productToRegister = JsonSerializer.Deserialize<ProductStockRegisterDto>(jsonContent);
                DtoValidator.Validate(productToRegister!);

                var productTitleDescription = new TitleDescriptionDto()
                {
                    Title = productToRegister!.Title,
                    Description = productToRegister!.Description
                };
                _logger.LogDebug($"{_className} - ProcessSingleProductFile - Product titled: {productTitleDescription.Title}");

                var product = await _apiService.Products.GetByTitleDescription(productTitleDescription, cancellationToken);
                if (product == null)
                {
                    var createProduct = _mapper.Map<CreateProductDto>(productToRegister);
                    var postedProduct = await _apiService.Products.PostWithFileNameAsync(createProduct, fileName, cancellationToken);
                    _logger.LogDebug($"{_className} - ProcessSingleProductFile - Posted product titled: {productTitleDescription.Title} - ID: {postedProduct!.Id} in database");
                    _fileService.MoveRegisteredProduct(currentFile);
                }
                else
                {
                    _logger.LogDebug($"{_className} - ProcessSingleProductFile - Product already posted, checking if file should be deleted...");
                    var processedFile = await _apiService.ProcessedFileLogs.GetByFileNameAsync(fileName, cancellationToken);
                    if(processedFile != null)
                    {
                        _fileService.MoveRegisteredProduct(currentFile);
                    }
                    else
                    {
                        _fileService.DeleteFile(currentFile);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"{_className} - ProcessSingleProductFile - Operation was cancelled for file: {fileName}");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleProductFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (InvalidFileException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleProductFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleProductFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleProductFile - Error: {ex}. It will be retried in the next iteration.");
            }
        }
    }
}
