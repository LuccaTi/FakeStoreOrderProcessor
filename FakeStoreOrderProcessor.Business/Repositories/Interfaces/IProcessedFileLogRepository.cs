using FakeStoreOrderProcessor.Library.DTO.ProcessedFileLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories.Interfaces
{
    public interface IProcessedFileLogRepository : IApiRepository<ProcessedFileLogDto, CreateProcessedFileLogDto, UpdateProcessedFileLogDto>
    {
        public Task<ProcessedFileLogDto?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken);
    }
}
