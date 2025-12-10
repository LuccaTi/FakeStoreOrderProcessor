using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories.Interfaces
{
    public interface IApiRepository<T> where T : class
    {
        public Task<List<T>?> GetAllAsync();
        public Task<T?> GetByIdAsync(long id);
        public Task<T?> PostAsync(T entity);
        public Task PatchAsync(long id, T entity);
        public Task DeleteAsync(long id);
    }
}
