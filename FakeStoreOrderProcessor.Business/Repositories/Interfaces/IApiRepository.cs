using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories.Interfaces
{
    public interface IApiRepository<TEntity, TCreateEntity, TUpdateEntity> 
        where TEntity : class
        where TCreateEntity : class
        where TUpdateEntity : class
    {
        public Task<List<TEntity>?> GetAllAsync();
        public Task<TEntity?> GetByIdAsync(long id);
        public Task<TEntity?> PostAsync(TCreateEntity entity);
        public Task PatchAsync(long id, TUpdateEntity entity);
        public Task DeleteAsync(long id);
    }
}
