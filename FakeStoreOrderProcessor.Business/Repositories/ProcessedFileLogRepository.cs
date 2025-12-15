using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using FakeStoreOrderProcessor.Library.DTO.ProcessedFileLog;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories
{
    public class ProcessedFileLogRepository : ApiRepository<ProcessedFileLogDto, CreateProcessedFileLogDto, UpdateProcessedFileLogDto>, IProcessedFileLogRepository
    {
        private const string _endpoint = "api/v1/ProcessedFileLog";

        public ProcessedFileLogRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory, _endpoint)
        {

        }

        public async Task<ProcessedFileLogDto?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"{_endpoint}/{fileName}", cancellationToken);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                if(ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            return await response.Content.ReadFromJsonAsync<ProcessedFileLogDto>(cancellationToken);
        }
    }
}
