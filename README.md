# FakeStoreOrderProcessor — Serviço Windows em .NET 8

## Visão geral
Este repositório contém um template de Serviço Windows construído com .NET 8 e TopShelf. O projeto já vem estruturado com camadas separadas (Host → Business → Library), configuração centralizada, logging com Serilog e execução baseada em timer.

A execução do trabalho é feita de forma assíncrona para maximizar a eficiência do uso dos recursos computacionais.

Funcionalidade de exemplo disponível:
- Timer configurável que executa threads de trabalho em intervalos definidos via `appsettings.json`.

## Tecnologias e bibliotecas essenciais
- .NET 8 (Console Application)
- TopShelf (hospedagem e instalação de serviços Windows)
- Serilog (logging para console e arquivo)
- Microsoft.Extensions.Configuration (leitura de `appsettings.json`)
- Microsoft.Extensions.DependencyInjection

## Estrutura do projeto
- `FakeStoreOrderProcessor.Host/Program.cs`: ponto de entrada; configura TopShelf, carrega configurações e inicializa o serviço.
- `FakeStoreOrderProcessor.Business/ServiceLifeCycleManager.cs`: classe principal que controla o ciclo de vida do serviço.
- `FakeStoreOrderProcessor.Business/ServiceProcessingOrchestrator.cs`: classe responsável por controlar o fluxo do trabalho.
- `FakeStoreOrderProcessor.Business/ServiceProcessingEngine.cs`: classe que vai obter os dados e aplicar a lógica de negócio.
- `FakeStoreOrderProcessor.Business/Configuration/Config.cs`: gerenciador centralizado de configurações via `appsettings.json`.
- `FakeStoreOrderProcessor.Business/Logging/Logger.cs`: inicialização e wrappers do Serilog com métodos `Debug`, `Info` e `Error`.
- `FakeStoreOrderProcessor.Library/Models/`: camada para modelos de dados (placeholder para futura implementação).
- `FakeStoreOrderProcessor.Host/appsettings.json`: configurações da aplicação (intervalo de execução, diretório de logs, níveis de log).

## Arquitetura e padrões de projeto
- Hospedagem e ciclo de vida
    - Usa TopShelf para facilitar instalação, execução e gerenciamento como serviço Windows.
    - Executa como `LocalSystem` por padrão.
    - Nome do serviço: `FakeStoreOrderProcessor`.
    - Callbacks de `WhenStarted` e `WhenStopped` para controle do ciclo de vida.

- Separação de camadas
    - **FakeStoreOrderProcessor.Host**: responsável apenas pela hospedagem e bootstrap.
    - **FakeStoreOrderProcessor.Business**: contém toda a lógica de negócio, configuração e logging.
    - **FakeStoreOrderProcessor.Library**: camada para modelos compartilhados.

- Logging (Serilog)
    - Logs em console e arquivo rolling diário em `logs/system_log_.txt` (diretório configurável).
    - Formato padronizado: `ClassName - MethodName - Message`.
    - Em falhas na inicialização, um arquivo é escrito em `StartupErrors/` para garantir rastreabilidade mesmo antes do logger estar ativo.

- Tratamento de erros
    - Exceções no startup são capturadas e registradas em arquivo dedicado antes de encerrar com código de saída diferente de zero.
    - Exceções durante execução são logadas e re-lançadas para que o TopShelf gerencie adequadamente.
    - Exceções de operações canceladas são propagadas porque servem como sinalização (metodologia assíncrona).

## Configuração
Arquivo: `FakeStoreOrderProcessor.Host/appsettings.json`

Seções disponíveis:
- **`AppLogging`**:
  - `LogDirectory` (string): pasta para gravação dos logs (relativa ao diretório base da aplicação).
  - `LogLevel` (string): nível mínimo de log ("Debug", "Information", "Warning", "Error").

- **`AppConfig`**:
  - `Interval` (int): intervalo em milissegundos entre execuções do timer (padrão: 10000 = 10 segundos).
  - `WriteLogConsole`: flag que indica se o serviço deve registrar os logs no console assim como faz nos arquivos.

## Uso e Instalação
O código precisa ser compilado tanto em versão Debug quanto versão Release para gerar o executável, em seguida pode rodar como console ao 
usar o .exe no terminal (cmd, por exemplo), também pode ser instalado ao adicionar o argumento "install", o mesmo se aplica para 
desinstalá-lo, porém o argumento é "uninstall".
