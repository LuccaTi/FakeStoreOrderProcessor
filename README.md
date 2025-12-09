# FakeStoreOrderProcessor — Serviço Windows em .NET 8

## Visão geral
Este repositório contém um template de Serviço Windows construído com .NET 8 e o Host genérico para serviços do .NET. O projeto já vem estruturado com camadas separadas (Host → Business → Library), configuração centralizada, logging com Serilog e execução baseada em timer.

A execução do trabalho é feita de forma assíncrona para maximizar a eficiência do uso dos recursos computacionais.

Funcionalidade de exemplo disponível:
- Timer configurável que executa threads de trabalho em intervalos definidos via `appsettings.json`.

## Tecnologias e bibliotecas essenciais
- .NET 8 (Console Application)
- Microsoft.Extensions.Hosting (hospedagem e instalação de serviços Windows)
- Serilog (logging para console e arquivo)
- Microsoft.Extensions.Configuration (leitura de `appsettings.json`)
- Microsoft.Extensions.DependencyInjection

## Estrutura do projeto
- `FakeStoreOrderProcessor.Host/Program.cs`: ponto de entrada; configura TopShelf, carrega configurações e inicializa o serviço.
- `FakeStoreOrderProcessor.Business/ServiceLifeCycleManager.cs`: classe principal que controla o ciclo de vida do serviço.
- `FakeStoreOrderProcessor.Business/Orchestrators/ServiceOrchestrator.cs`: classe responsável por controlar o fluxo do trabalho.
- `FakeStoreOrderProcessor.Business/Engines/ServiceEngine.cs`: classe que vai obter os dados e aplicar a lógica de negócio.
- `FakeStoreOrderProcessor.Library/Models/`: camada para modelos de dados (placeholder para futura implementação).
- `FakeStoreOrderProcessor.Host/appsettings.json`: configurações da aplicação (intervalo de execução, diretório de logs, níveis de log).

## Arquitetura e padrões de projeto
- Hospedagem e ciclo de vida
    - Usa o host genérico de serviços da Microsoft para facilitar instalação, execução e gerenciamento como serviço Windows.
    - Nome do serviço: `FakeStoreOrderProcessor`.
    - Callbacks de `ExecuteAsync` e `StopAsync` para controle do ciclo de vida.

- Separação de camadas
    - **FakeStoreOrderProcessor.Host**: responsável apenas pela hospedagem e bootstrap.
    - **FakeStoreOrderProcessor.Business**: contém toda a lógica de negócio.
    - **FakeStoreOrderProcessor.Library**: camada para modelos compartilhados.

- Logging (Serilog)
    - Logs em console e arquivo rolling diário em `logs/system_log_.txt`, a pasta `logs` fica no diretório base da aplicação.
    - Em falhas na inicialização, um arquivo é escrito na pasta logs usando um bootstrap logger para garantir rastreabilidade mesmo antes do logger principal estar ativo.

- Tratamento de erros
    - Exceções no startup são capturadas e registradas em arquivo dedicado antes de encerrar a aplicação.
    - Exceções durante a execução que não forem tratadas são registradas nos logs.
    - O tratamento de erros do serviço impede que seja encerrado diante de exceções.

## Configuração
Arquivo: `FakeStoreOrderProcessor.Host/appsettings.json`

Seções disponíveis:
- **`ServiceSettings`**:
  - `Interval` (int): intervalo em segundos entre execuções do timer (padrão: 10 segundos).

- **`Serilog`**:
  - `MinimumLevel` (string): nível mínimo de log ("Debug", "Information", "Warning", "Error")
  - `WriteTo`: (string): determina que o log também é escrito no console.

## Uso e Instalação
O código precisa ser compilado tanto em versão Debug quanto versão Release para gerar o executável, em seguida pode rodar como console ao usar o .exe no terminal (cmd, por exemplo).

Para instalar é preciso seguir o passo a passo abaixo:

- No Visual Studio, compile a solução na configuração Release.

- Localize o caminho completo do executável gerado. 
Exemplo: C:\seus\projetos\solução\projeto com Program.cs\bin\Release\net8.0\service.exe.

- Abra o cmd ou o power shell como administrador e execute o comando abaixo para criar o serviço no windows:
sc.exe create "ServiceName" binPath="C:\caminho\completo\para\seu\service.exe" DisplayName="Service Display Name"

- Use o comando abaixo para adicionar uma descrição:
sc.exe description "ServiceName" "Descrição o serviço.".

- Inicie o serviço via linha de comando:
sc.exe start "ServiceName"

- Para desinstalar, primeiro pare o serviço:
sc.exe stop "ServiceName"

- Em seguida desinstale:
sc.exe delete "ServiceName"
