using FakeStoreOrderProcessor.Library.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories.Interfaces
{
    public interface IProductRepository : IApiRepository<ProductDto, CreateProductDto, UpdateProductDto>
    {
        public Task<ProductDto?> GetByTitleDescription(TitleDescriptionDto titleDescriptionDto, CancellationToken cancellationToken);
        public Task<ProductDto?> PostWithFileNameAsync(CreateProductDto productDto, string fileName, CancellationToken cancellationToken);
        public Task<bool> ProductExistsAsync(long id, CancellationToken cancellationToken);
    }
}
