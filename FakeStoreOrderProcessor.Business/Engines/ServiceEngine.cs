using AutoMapper;
using FakeStoreOrderProcessor.Business.Engines.Interfaces;
using FakeStoreOrderProcessor.Business.Exceptions;
using FakeStoreOrderProcessor.Business.Services.Interfaces;
using FakeStoreOrderProcessor.Library.DTO.Address;
using FakeStoreOrderProcessor.Library.DTO.Customer;
using FakeStoreOrderProcessor.Library.DTO.Json;
using FakeStoreOrderProcessor.Library.DTO.Order;
using FakeStoreOrderProcessor.Library.DTO.Product;
using FakeStoreOrderProcessor.Library.Enums;
using FakeStoreOrderProcessor.Library.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace FakeStoreOrderProcessor.Business.Engines
{
    public class ServiceEngine : IServiceEngine
    {
        private const string _className = "ServiceEngine";
        private readonly ILogger<ServiceEngine> _logger;
        private readonly IOptions<ServiceSettings> _settings;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IApiService _apiService;
        private DateTime _lastCancelOrdersRun = DateTime.MinValue;

        public ServiceEngine(ILogger<ServiceEngine> logger, IOptions<ServiceSettings> settings, IMapper mapper, IFileService fileService, IApiService apiService)
        {
            _logger = logger;
            _settings = settings;
            _mapper = mapper;
            _fileService = fileService;
            _apiService = apiService;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            if (_lastCancelOrdersRun.Date < DateTime.Today)
            {
                _logger.LogInformation($"{_className} - Running daily CancelOrders task.");
                await CancelOrders(cancellationToken);
                _lastCancelOrdersRun = DateTime.Now;
                _logger.LogInformation($"{_className} - Daily CancelOrders task finished.");
            }

            var processingFiles = _fileService.GetAllProcessingFiles();
            var files = _fileService.GetAllFiles();

            if (processingFiles.Any())
            {

                _logger.LogDebug($"{_className} - ProcessAsync - Starting processing pending files");

                var processingProductFiles = processingFiles.Where(file => Path.GetFileName(file).Contains("product")).ToList();
                foreach (var file in processingProductFiles)
                {
                    await ProcessSingleProductFile(file, false, cancellationToken);
                }

                var processingOrderCreatedFiles = processingFiles.Where(file => Path.GetFileName(file).Contains("created")).ToList();
                foreach (var file in processingOrderCreatedFiles)
                {
                    await ProcessSingleOrderCreatedFile(file, false, cancellationToken);
                }

                var processingOrderPaymentFiles = processingFiles.Where(file => Path.GetFileName(file).Contains("payment")).ToList();
                foreach (var file in processingOrderPaymentFiles)
                {
                    await ProcessSingleOrderPaymentFile(file, false, cancellationToken);
                }

                var processingOrderShippedFiles = processingFiles.Where(file => Path.GetFileName(file).Contains("shipped")).ToList();
                foreach (var file in processingOrderShippedFiles)
                {
                    await ProcessSingleOrderShippedFile(file, false, cancellationToken);
                }

                var processingOrderDeliveredFiles = processingFiles.Where(file => Path.GetFileName(file).Contains("delivered")).ToList();
                foreach (var file in processingOrderDeliveredFiles)
                {
                    await ProcessSingleOrderDeliveredFile(file, false, cancellationToken);
                }
            }

            if (files.Any())
            {

                _logger.LogDebug($"{_className} - ProcessAsync - Starting processing");

                var productFiles = files.Where(file => Path.GetFileName(file).Contains("product")).ToList();
                foreach (var file in productFiles)
                {
                    await ProcessSingleProductFile(file, true, cancellationToken);
                }

                var orderCreatedFiles = files.Where(file => Path.GetFileName(file).Contains("created")).ToList();
                foreach (var file in orderCreatedFiles)
                {
                    await ProcessSingleOrderCreatedFile(file, true, cancellationToken);
                }

                var orderPaymentFiles = files.Where(file => Path.GetFileName(file).Contains("payment")).ToList();
                foreach (var file in orderPaymentFiles)
                {
                    await ProcessSingleOrderPaymentFile(file, true, cancellationToken);
                }

                var OrderShippedFiles = files.Where(file => Path.GetFileName(file).Contains("shipped")).ToList();
                foreach (var file in OrderShippedFiles)
                {
                    await ProcessSingleOrderShippedFile(file, true, cancellationToken);
                }

                var OrderDeliveredFiles = files.Where(file => Path.GetFileName(file).Contains("delivered")).ToList();
                foreach (var file in OrderDeliveredFiles)
                {
                    await ProcessSingleOrderDeliveredFile(file, true, cancellationToken);
                }
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
                    if (processedFile != null)
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

        public async Task ProcessSingleOrderCreatedFile(string file, bool moveToProcessing, CancellationToken cancellationToken)
        {
            string currentFile = file;
            string fileName = Path.GetFileName(file);
            _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Processing file: {fileName}");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (moveToProcessing)
                    currentFile = _fileService.MoveToProcessing(file);

                string jsonContent = await File.ReadAllTextAsync(currentFile, cancellationToken);

                var serializerOptions = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                var orderToRegister = JsonSerializer.Deserialize<OrderCreatedDto>(jsonContent, serializerOptions);
                DtoValidator.Validate(orderToRegister!);

                await ProcessAddress(orderToRegister, cancellationToken);

                string street = orderToRegister!.Customer!.Address!.Street!;
                int number = orderToRegister.Customer.Address.Number;

                await ProcessCustomer(orderToRegister, street, number, cancellationToken);

                var orderToPost = _mapper.Map<CreateOrderDto>(orderToRegister);
                _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Processing order with guid: {orderToPost.OrderGuid}");

                var loginRequest = new LoginRequestDto()
                {
                    Username = orderToRegister!.Customer!.Username,
                    Password = orderToRegister!.Customer!.Password
                };

                var customer = await _apiService.Customers.GetByLoginRequestAsync(loginRequest, cancellationToken);
                orderToPost.CustomerId = customer!.Id;
                _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Customer ID obtained");

                foreach (var item in orderToRegister.Items!)
                {
                    var titleDescription = new TitleDescriptionDto()
                    {
                        Title = item.Title,
                        Description = item.Description
                    };

                    var product = await _apiService.Products.GetByTitleDescription(titleDescription, cancellationToken);
                    var orderItem = _mapper.Map<CreateOrderItemDto>(item);
                    orderItem.ProductId = product!.Id;
                    orderToPost.OrderItems.Add(orderItem);
                }
                _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Order items added");

                var orderDto = await _apiService.Orders.GetByGuidAsync(orderToPost.OrderGuid!, cancellationToken);
                if (orderDto == null)
                {
                    await _apiService.Orders.PostAsync(orderToPost, cancellationToken);
                    _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Order posted");
                }
                else
                {
                    _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Order already posted");
                }

                _fileService.MoveProcessedFile(currentFile);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"{_className} - ProcessSingleOrderCreatedFile - Operation was cancelled for file: {fileName}");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderCreatedFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (InvalidFileException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderCreatedFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderCreatedFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderCreatedFile - Error: {ex}. It will be retried in the next iteration.");
            }
        }

        public async Task ProcessSingleOrderPaymentFile(string file, bool moveToProcessing, CancellationToken cancellationToken)
        {
            string currentFile = file;
            string fileName = Path.GetFileName(file);
            _logger.LogDebug($"{_className} - ProcessSingleOrderPaymentFile - Processing file: {fileName}");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (moveToProcessing)
                    currentFile = _fileService.MoveToProcessing(file);

                string jsonContent = await File.ReadAllTextAsync(currentFile, cancellationToken);

                var serializerOptions = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                var orderPayed = JsonSerializer.Deserialize<OrderPaymentConfirmedDto>(jsonContent, serializerOptions);
                DtoValidator.Validate(orderPayed!);

                _logger.LogDebug($"{_className} - ProcessSingleOrderPaymentFile - Patching order with guid: {orderPayed!.OrderGuid}");

                var orderToPatch = await _apiService.Orders.GetByGuidWithOrderItemsAsync(orderPayed!.OrderGuid!, cancellationToken);
                var patchedOrder = _mapper.Map<UpdateOrderDto>(orderPayed);
                foreach (var item in orderToPatch!.OrderItems)
                {
                    patchedOrder.OrderItems.Add(new CreateOrderItemDto()
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice
                    });
                }
                patchedOrder.OrderDate = orderToPatch.OrderDate;

                await _apiService.Orders.PatchOrderAsync(orderPayed!.OrderGuid!, patchedOrder, cancellationToken);
                _logger.LogDebug($"{_className} - ProcessSingleOrderPaymentFile - Order patched");

                _fileService.MoveProcessedFile(currentFile);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"{_className} - ProcessSingleOrderPaymentFile - Operation was cancelled for file: {fileName}");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderPaymentFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (InvalidFileException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderPaymentFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderPaymentFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderPaymentFile - Error: {ex}. It will be retried in the next iteration.");
            }
        }

        public async Task ProcessSingleOrderShippedFile(string file, bool moveToProcessing, CancellationToken cancellationToken)
        {
            string currentFile = file;
            string fileName = Path.GetFileName(file);
            _logger.LogDebug($"{_className} - ProcessSingleOrderShippedFile - Processing file: {fileName}");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (moveToProcessing)
                    currentFile = _fileService.MoveToProcessing(file);

                string jsonContent = await File.ReadAllTextAsync(currentFile, cancellationToken);

                var serializerOptions = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                var orderShipped = JsonSerializer.Deserialize<OrderShippedDto>(jsonContent, serializerOptions);
                DtoValidator.Validate(orderShipped!);

                _logger.LogDebug($"{_className} - ProcessSingleOrderShippedFile - Patching order with guid: {orderShipped!.OrderGuid}");

                var orderToPatch = await _apiService.Orders.GetByGuidWithOrderItemsAsync(orderShipped!.OrderGuid!, cancellationToken);
                var patchedOrder = _mapper.Map<UpdateOrderDto>(orderShipped);
                foreach (var item in orderToPatch!.OrderItems)
                {
                    patchedOrder.OrderItems.Add(new CreateOrderItemDto()
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice
                    });
                }
                patchedOrder.OrderDate = orderToPatch.OrderDate;
                patchedOrder.PaymentDate = orderToPatch.PaymentDate;
                patchedOrder.TotalPrice = orderToPatch.TotalPrice;

                await _apiService.Orders.PatchOrderAsync(orderShipped!.OrderGuid!, patchedOrder, cancellationToken);
                _logger.LogDebug($"{_className} - ProcessSingleOrderShippedFile - Order patched");

                _fileService.MoveProcessedFile(currentFile);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"{_className} - ProcessSingleOrderShippedFile - Operation was cancelled for file: {fileName}");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderShippedFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (InvalidFileException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderShippedFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderShippedFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderShippedFile - Error: {ex}. It will be retried in the next iteration.");
            }
        }

        public async Task ProcessSingleOrderDeliveredFile(string file, bool moveToProcessing, CancellationToken cancellationToken)
        {
            string currentFile = file;
            string fileName = Path.GetFileName(file);
            _logger.LogDebug($"{_className} - ProcessSingleOrderDeliveredFile - Processing file: {fileName}");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (moveToProcessing)
                    currentFile = _fileService.MoveToProcessing(file);

                string jsonContent = await File.ReadAllTextAsync(currentFile, cancellationToken);

                var serializerOptions = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                var orderDelivered = JsonSerializer.Deserialize<OrderDeliveredDto>(jsonContent, serializerOptions);
                DtoValidator.Validate(orderDelivered!);

                _logger.LogDebug($"{_className} - ProcessSingleOrderDeliveredFile - Patching order with guid: {orderDelivered!.OrderGuid}");

                var orderToPatch = await _apiService.Orders.GetByGuidWithOrderItemsAsync(orderDelivered!.OrderGuid!, cancellationToken);
                var patchedOrder = _mapper.Map<UpdateOrderDto>(orderDelivered);
                foreach (var item in orderToPatch!.OrderItems)
                {
                    patchedOrder.OrderItems.Add(new CreateOrderItemDto()
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice
                    });
                }
                patchedOrder.OrderDate = orderToPatch.OrderDate;
                patchedOrder.PaymentDate = orderToPatch.PaymentDate;
                patchedOrder.ShippedDate = orderToPatch.ShippedDate;
                patchedOrder.TotalPrice = orderToPatch.TotalPrice;

                await _apiService.Orders.PatchOrderAsync(orderDelivered!.OrderGuid!, patchedOrder, cancellationToken);
                _logger.LogDebug($"{_className} - ProcessSingleOrderDeliveredFile - Order patched");

                _fileService.MoveProcessedFile(currentFile);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"{_className} - ProcessSingleOrderDeliveredFile - Operation was cancelled for file: {fileName}");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderDeliveredFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (InvalidFileException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderDeliveredFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (ValidationException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderDeliveredFile - Error: {ex.Message}");
                _fileService.MoveInvalidFile(currentFile);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"{_className} - ProcessSingleOrderDeliveredFile - Error: {ex}. It will be retried in the next iteration.");
            }
        }

        public async Task ProcessAddress(OrderCreatedDto? orderToRegister, CancellationToken cancellationToken)
        {
            var streetNumber = new StreetNumberDto()
            {
                Street = orderToRegister!.Customer!.Address!.Street,
                Number = orderToRegister.Customer.Address.Number
            };
            var address = await _apiService.Addresses.GetByStreetNumberAsync(streetNumber, cancellationToken);
            if (address == null)
            {
                _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Posting address with street: {streetNumber.Street} and number: {streetNumber.Number}");
                var addressToPost = _mapper.Map<CreateAddressDto>(orderToRegister.Customer.Address);
                var postedAddress = await _apiService.Addresses.PostAsync(addressToPost, cancellationToken);
                _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFIle - Address posted");
            }
        }

        public async Task ProcessCustomer(OrderCreatedDto? orderToRegister, string street, int number, CancellationToken cancellationToken)
        {
            var loginRequest = new LoginRequestDto()
            {
                Username = orderToRegister!.Customer!.Username,
                Password = orderToRegister!.Customer!.Password
            };
            var customer = await _apiService.Customers.GetByLoginRequestAsync(loginRequest, cancellationToken);
            if (customer == null)
            {
                _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Posting customer with username: {orderToRegister.Customer.Username}");
                var customerToPost = _mapper.Map<CreateCustomerDto>(orderToRegister.Customer);

                var streetNumber = new StreetNumberDto()
                {
                    Street = street,
                    Number = number
                };

                customerToPost.FirstName = orderToRegister.Customer.Name!.FirstName;
                customerToPost.LastName = orderToRegister.Customer.Name!.LastName;
                var tempAddress = await _apiService.Addresses.GetByStreetNumberAsync(streetNumber, cancellationToken);
                customerToPost.AddressId = tempAddress!.Id;

                var postedCustomer = await _apiService.Customers.PostAsync(customerToPost, cancellationToken);
                _logger.LogDebug($"{_className} - ProcessSingleOrderCreatedFile - Customer posted");
            }
        }

        public async Task CancelOrders(CancellationToken cancellationToken)
        {
            try
            {
                var previousDayOrders = await _apiService.Orders.GetAllDayBeforeAsync(cancellationToken);
                var notPayedOrders = previousDayOrders.Where(o => o.PaymentStatus == PaymentStatus.Pending.ToString()).ToList();

                foreach (var order in notPayedOrders)
                {
                    try
                    {
                        var currentMonthProcessedFolder = Path.Combine(_fileService.ProcessedFilesFolder!, DateTime.Now.ToString("yyyy/MM")).Replace(@"/", "\\");
                        var orderFile = Directory.EnumerateFiles(currentMonthProcessedFolder, "*.*", SearchOption.AllDirectories)
                                    .Where(file => Path.GetFileName(file)
                                    .Contains(order.OrderGuid!)).FirstOrDefault();

                        if (order.PaymentStatus == PaymentStatus.Pending.ToString() && order.OrderStatus != OrderStatus.Cancelled.ToString())
                        {
                            var patchedOrder = _mapper.Map<UpdateOrderDto>(order);
                            var orderWithItems = await _apiService.Orders.GetByGuidWithOrderItemsAsync(order.OrderGuid!, cancellationToken);

                            foreach (var item in orderWithItems!.OrderItems)
                            {
                                patchedOrder.OrderItems.Add(new CreateOrderItemDto()
                                {
                                    ProductId = item.ProductId,
                                    Quantity = item.Quantity,
                                    TotalPrice = item.TotalPrice
                                });
                            }

                            patchedOrder.OrderStatus = OrderStatus.Cancelled.ToString();

                            await _apiService.Orders.PatchOrderAsync(order.OrderGuid!, patchedOrder!, cancellationToken);
                        }

                        try
                        {
                            await _apiService.Orders.DeleteWithGuidAsync(order.OrderGuid!, cancellationToken);
                        }
                        catch (HttpRequestException ex)
                        {
                            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                _logger.LogDebug($"{_className} - CancelOrders - Order with guid: {order.OrderGuid} already soft deleted from database");
                            }
                            else
                            {
                                throw;
                            }
                        }

                        if (!string.IsNullOrEmpty(orderFile))
                        {
                            _fileService.MoveCancelledOrder(orderFile!);
                        }
                        else
                        {
                            _logger.LogWarning($"Order with guid: {order.OrderGuid} does not have a file in processed folder");
                        }
                    }
                    catch (InvalidOrderException ex)
                    {
                        _logger.LogError($"{_className} - CancelOrders - Error: {ex.Message}");
                        continue;
                    }
                }

                if (_settings.Value.CancelOtherOrders)
                {
                    var AllOrders = await _apiService.Orders.GetAllActiveOrNotAsync(cancellationToken);
                    var ordersToCancel = AllOrders!.Where(o => o.PaymentStatus == PaymentStatus.Pending.ToString()).ToList();

                    foreach (var order in ordersToCancel)
                    {
                        try
                        {
                            _logger.LogDebug($"{_className} - CancelOrders - Order with GUID: '{order.OrderGuid}'");
                            var currentMonthProcessedFolder = Path.Combine(_fileService.ProcessedFilesFolder!, DateTime.Now.ToString("yyyy/MM")).Replace(@"/", "\\");
                            var orderFile = Directory.EnumerateFiles(currentMonthProcessedFolder, "*.*", SearchOption.AllDirectories)
                                        .Where(file => Path.GetFileName(file)
                                        .Contains(order.OrderGuid!)).FirstOrDefault();

                            if (order.PaymentStatus == PaymentStatus.Pending.ToString() && order.OrderStatus != OrderStatus.Cancelled.ToString())
                            {
                                var patchedOrder = _mapper.Map<UpdateOrderDto>(order);
                                var orderWithItems = await _apiService.Orders.GetByGuidWithOrderItemsAsync(order.OrderGuid!, cancellationToken);

                                foreach (var item in orderWithItems!.OrderItems)
                                {
                                    patchedOrder.OrderItems.Add(new CreateOrderItemDto()
                                    {
                                        ProductId = item.ProductId,
                                        Quantity = item.Quantity,
                                        TotalPrice = item.TotalPrice
                                    });
                                }

                                patchedOrder.OrderStatus = OrderStatus.Cancelled.ToString();

                                await _apiService.Orders.PatchOrderAsync(order.OrderGuid!, patchedOrder!, cancellationToken);
                                _logger.LogDebug($"{_className} - CancelOrders - Order patched in DB");
                            }

                            try
                            {
                                await _apiService.Orders.DeleteWithGuidAsync(order.OrderGuid!, cancellationToken);
                                _logger.LogDebug($"{_className} - CancelOrders - Order soft deleted");
                            }
                            catch (HttpRequestException ex)
                            {
                                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                                {
                                    _logger.LogDebug($"{_className} - CancelOrders - Order with guid: {order.OrderGuid} already soft deleted from database");
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            if (!string.IsNullOrEmpty(orderFile))
                            {
                                _fileService.MoveCancelledOrder(orderFile!);
                                _logger.LogDebug($"{_className} - CancelOrders - Order moved");
                            }
                            else
                            {
                                _logger.LogWarning($"Order with guid: {order.OrderGuid} does not have a file in processed folder");
                            }

                            _logger.LogDebug($"{_className} - CancelOrders - Order cancelled");
                        }
                        catch (InvalidOrderException ex)
                        {
                            _logger.LogError($"{_className} - CancelOrders - Error: {ex.Message}");
                            continue;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"{_className} - CancelOrders - Error: {ex}. It will be retried in the next iteration.");
                throw;
            }
        }
    }
}
