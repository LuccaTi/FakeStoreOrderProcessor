using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeStoreOrderProcessor.Business.Engines.Interfaces;
using FakeStoreOrderProcessor.Business.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FakeStoreOrderProcessor.Business.Engines
{
    public class ServiceEngine : IServiceEngine
    {
        private const string _className = "ServiceEngine";
        private readonly ILogger<ServiceEngine> _logger;
        private readonly IFileService _fileService;
        public ServiceEngine(ILogger<ServiceEngine> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{_className} - Service testing...");
            /*while (!cancellationToken.IsCancellationRequested)
            {
            Crio um método de processamento para cada tipo de arquivo - Será feito pelo engine ao juntar o trabalho do fileservice com apiservice
            O fluxo será, tenta processar o arquivo e enviar os dados para a API,
            se houver erro para processar o arquivo, envia para pasta de arquivos inválidos,
            se o erro for na comunicação com a API, passa para o próximo arquivo,
            dependendo dos erros vários fluxos diferentes podem ser criados.
           
            }*/
        }
    }
}
