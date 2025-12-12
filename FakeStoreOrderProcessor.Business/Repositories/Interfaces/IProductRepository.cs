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
        public Task<ProductDto?> GetByTitleDescription(TitleDescriptionDto titleDescriptionDto);
        public Task<bool> ProductExistsAsync(long id);
    }
}
