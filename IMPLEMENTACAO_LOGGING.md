# Implementação de Logging com Serilog

## Resumo das Mudanças

Este documento descreve a implementação completa do sistema de logging estruturado usando Serilog no projeto GitActDash, fornecendo observabilidade abrangente para debugging, monitoramento e auditoria.

## Pacotes Instalados

### 📦 **Serilog.AspNetCore** (v9.0.0)
- Integração completa com ASP.NET Core
- Suporte a DI e configuração
- Substituição do logging padrão

### 📦 **Serilog.Sinks.File** (v7.0.0)
- Gravação de logs em arquivos
- Rotação diária automática
- Retenção configurável de arquivos

### 📦 **Serilog.Enrichers.Environment** (v3.0.1)
- Enriquecimento com informações do ambiente
- Nome da máquina e ambiente de execução

## Configuração Implementada

### 🔧 **Program.cs**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(...)
    .WriteTo.File(...)
    .CreateLogger();
```

### 📁 **Estrutura de Logs**
- **Console**: Logs formatados para desenvolvimento
- **Arquivo**: Logs estruturados com rotação diária
- **Localização**: `logs/gitactdash-YYYY-MM-DD.log`
- **Retenção**: 31 dias
- **Formato**: Timestamp detalhado + Context + Propriedades

## Classes Implementadas

### 🛠️ **LoggingExtensions.cs**

#### **Métodos de Contexto**
```csharp
// Operações do GitHub com contexto
using var _ = logger.ForGitHubOperation("GetRepositories", repository: "repo-name", organization: "org-name");

// Operações de serviço
using var serviceContext = logger.ForServiceOperation(nameof(GitHubService), nameof(GetUserRepositoriesAsync));

// Operações de componente
using var componentContext = logger.ForComponentOperation("FilterPanel", "LoadRepositories");
```

#### **Temporização de Operações**
```csharp
using var timer = logger.TimeOperation("GetUserRepositories");
// Operação sendo medida
// Ao final do using, o tempo é automaticamente logado
```

#### **Classes Auxiliares**
- **CompositeDisposable**: Combina múltiplos contextos
- **OperationTimer<T>**: Mede e loga tempo de execução automaticamente

## Implementação nos Services

### 🔍 **GitHubService**
```csharp
public async Task<OperationResult<IReadOnlyList<Repository>>> GetUserRepositoriesAsync(CancellationToken cancellationToken = default)
{
    using var _ = logger.ForServiceOperation(nameof(GitHubService), nameof(GetUserRepositoriesAsync));
    using var timer = logger.TimeOperation("GetUserRepositories");

    logger.LogInformation("Starting to fetch user repositories");
    
    try
    {
        // Operação principal
        logger.LogDebug("Fetched {Count} personal repositories", userRepos.Count);
        logger.LogInformation("Successfully fetched {TotalCount} repositories ({UniqueCount} unique)", 
            allRepos.Count, distinctRepos.Length);
    }
    catch (RateLimitExceededException ex)
    {
        logger.LogWarning("GitHub API rate limit exceeded. Reset at: {ResetTime}", ex.Reset);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error while fetching repositories");
    }
}
```

### 💾 **LocalStorageService**
```csharp
public async Task<OperationResult> SetItemAsync(string key, string value, CancellationToken cancellationToken = default)
{
    using var _ = logger.ForServiceOperation(nameof(LocalStorageService), nameof(SetItemAsync));
    
    logger.LogDebug("Setting localStorage item with key: {Key}", key);
    
    try
    {
        // Operação JavaScript
        logger.LogDebug("Successfully set localStorage item with key: {Key}", key);
    }
    catch (JSException ex)
    {
        logger.LogError(ex, "JavaScript error while setting localStorage item with key: {Key}", key);
    }
}
```

## Níveis de Log Implementados

### 📊 **Debug**
- Informações detalhadas de fluxo
- Contagens de itens processados
- Estados internos de operações

### 📋 **Information**
- Início e fim de operações principais
- Resultados de sucesso com contadores
- Tempo de execução de operações

### ⚠️ **Warning**
- Rate limits da API GitHub
- Operações canceladas
- Recursos inacessíveis mas não críticos

### ❌ **Error**
- Exceções capturadas com contexto
- Falhas de API com códigos de status
- Erros JavaScript do lado cliente

### 💥 **Fatal**
- Falhas críticas de inicialização
- Terminação inesperada da aplicação

## Contextos Estruturados

### 🏷️ **Propriedades Automáticas**
- **Environment**: Desenvolvimento/Produção
- **MachineName**: Identificação do servidor
- **SourceContext**: Nome da classe que originou o log

### 🎯 **Propriedades Customizadas**
- **Service**: Nome do serviço (GitHubService, LocalStorageService)
- **Operation**: Nome do método sendo executado
- **Repository**: Nome do repositório GitHub (quando aplicável)
- **Organization**: Nome da organização GitHub (quando aplicável)
- **Component**: Nome do componente Blazor (quando aplicável)

## Configuração de Appsettings

### ⚙️ **appsettings.json**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/gitactdash-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 31
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithEnvironmentName", "WithMachineName"]
  }
}
```

## Benefícios Implementados

### 🔍 **Debugging Aprimorado**
- Contexto detalhado de cada operação
- Rastreamento de chamadas GitHub API
- Identificação rápida de problemas

### 📈 **Monitoramento de Performance**
- Tempo de execução de operações
- Identificação de gargalos
- Métricas de uso da API

### 🛡️ **Auditoria e Compliance**
- Logs estruturados para análise
- Retenção configurável
- Não exposição de tokens sensíveis

### 🚀 **Produção Ready**
- Rotação automática de logs
- Configuração por ambiente
- Formato estruturado para ferramentas de análise

## Exemplos de Output

### 💻 **Console (Desenvolvimento)**
```
[14:32:15 INF] Starting to fetch user repositories {"Service": "GitHubService", "Operation": "GetUserRepositoriesAsync"}
[14:32:15 DBG] Fetched 12 personal repositories {"Service": "GitHubService", "Operation": "GetUserRepositoriesAsync"}
[14:32:16 INF] Completed operation: GetUserRepositories in 834.2ms {"Service": "GitHubService", "Operation": "GetUserRepositoriesAsync"}
```

### 📄 **Arquivo (Produção)**
```
2025-07-19 14:32:15.123 -03:00 [INF] GitActDashNet.Services.GitHubService: Starting to fetch user repositories {"Service": "GitHubService", "Operation": "GetUserRepositoriesAsync", "EnvironmentName": "Production", "MachineName": "WEB01"}
```

### ⚠️ **Aviso de Rate Limit**
```
[14:33:20 WRN] GitHub API rate limit exceeded. Reset at: 07/19/2025 15:33:20 {"Service": "GitHubService", "Operation": "GetUserRepositoriesAsync", "ResetTime": "2025-07-19T15:33:20Z"}
```

## Próximos Passos

1. **Implementar logging nos componentes Blazor** quando criados
2. **Adicionar métricas específicas** para operações críticas
3. **Configurar alertas** baseados em logs de erro
4. **Implementar correlation IDs** para rastreamento de requests

Esta implementação fornece uma base sólida para observabilidade, facilitando debugging, monitoramento e manutenção do sistema em produção.
