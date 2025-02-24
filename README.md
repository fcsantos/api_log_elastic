# API Logging Elastic

API desenvolvida em .NET 8 para centralizar logs de múltiplos projetos usando Elasticsearch. Esta API atua como um centralizador de logs, permitindo que outras aplicações enviem seus logs através de requisições HTTP.

## 🚀 Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/install/)

## 📦 Estrutura do Projeto

```
APILoggingElastic/
├── APILoggingElastic/
│     ├── Controllers/
│     ├── Models/
|     ├── Dockerfile
│     └── Program.cs
├── docker-compose.yml
├── docker-compose.override.yml
└── README.md
```

## 🔗 Integrando com Outros Projetos

### Cliente HTTP em .NET

1. Crie uma classe para o serviço de logs:

```csharp
public class LogService
{
    private readonly HttpClient _httpClient;
    private const string LogApiUrl = "http://seu-servidor:8080/api/log";

    public LogService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task LogAsync(string level, string message, string application, object additionalData)
    {
        var logModel = new
        {
            Level = level,
            Message = message,
            Application = application,
            AdditionalData = additionalData
        };

        await _httpClient.PostAsJsonAsync(LogApiUrl, logModel);
    }
}
```

2. Configure a injeção de dependência:

```csharp
services.AddHttpClient<LogService>();
```

3. Use em seus controllers/serviços:

```csharp
public class SeuController : ControllerBase
{
    private readonly LogService _logService;

    public SeuController(LogService logService)
    {
        _logService = logService;
    }

    public async Task<IActionResult> SeuMetodo()
    {
        try
        {
            // Seu código
            await _logService.LogAsync("information", "Operação realizada", "NomeDaSuaAplicacao", 
                new { userId = 123, action = "create" });
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("error", ex.Message, "NomeDaSuaAplicacao", 
                new { stack = ex.StackTrace });
            throw;
        }
    }
}
```

### Middleware para Logs Automáticos

```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LogService _logService;

    public LoggingMiddleware(RequestDelegate next, LogService logService)
    {
        _next = next;
        _logService = logService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Log requisições bem-sucedidas
            await _logService.LogAsync("information", 
                $"Request {context.Request.Method} {context.Request.Path} completed with status {context.Response.StatusCode}",
                "NomeDaSuaAplicacao",
                new { 
                    path = context.Request.Path,
                    method = context.Request.Method,
                    statusCode = context.Response.StatusCode
                });
        }
        catch (Exception ex)
        {
            // Log erros
            await _logService.LogAsync("error", 
                $"Request {context.Request.Method} {context.Request.Path} failed",
                "NomeDaSuaAplicacao",
                new { 
                    error = ex.Message,
                    stack = ex.StackTrace
                });
            throw;
        }
    }
}

// Extensão para configurar o middleware
public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingMiddleware>();
    }
}
```

### Usar o Middleware na Startup:

```csharp
app.UseLoggingMiddleware();
```

## 🔧 Configuração Local

1. Clone o repositório
```bash
git clone [URL_DO_REPOSITORIO]
cd APILoggingElastic
```

2. Restaure os pacotes NuGet
```bash
dotnet restore
```

3. Inicie os containers Docker
```bash
docker-compose up -d
```

## 📝 Endpoints

### Logs
POST /api/log
```json
{
  "application": "Nome da Aplicação",
  "level": "error|warning|information",
  "message": "Mensagem do log",
  "additionalData": {
    "propriedade1": "valor1",
    "propriedade2": "valor2"
  }
}
```

## 📊 Visualização dos Logs

1. Acesse o Kibana:
```
http://localhost:5601
```

2. Configure o index pattern:
- Menu > Stack Management > Index Patterns
- Create index pattern
- Pattern name: logs-api-*
- Time field: @timestamp

3. Visualize os logs:
- Menu > Analytics > Discover

## 🔍 Monitoramento

- Elasticsearch: http://localhost:9200
- Kibana: http://localhost:5601
- API: http://localhost:8080/swagger

## 🛠️ Desenvolvimento

Para adicionar novos logs, use o LogController como exemplo:

```csharp
private readonly ILogger<SeuController> _logger;

public SeuController(ILogger<SeuController> logger)
{
    _logger = logger;
}

// Exemplo de uso
_logger.LogInformation("Sua mensagem de log");
```

## 🐳 Docker

O projeto inclui configurações Docker para todos os serviços:

- API (.NET 8)
- Elasticsearch
- Kibana

Para reconstruir os containers após alterações:
```bash
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

## ⚙️ Configurações

Principais configurações no `appsettings.json`:

```json
{
  "ElasticsearchSettings": {
    "Uri": "http://elasticsearch:9200"
  }
}
```

## 🤝 Contribuição

1. Faça o fork do projeto
2. Crie sua feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença [MIT](https://opensource.org/licenses/MIT).
