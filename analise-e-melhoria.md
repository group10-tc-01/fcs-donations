# Análise e Melhorias — Fcg.Donations

## 1. Registro duplicado de dependências (SqlServer + MongoDb)

**Problema:** Tanto `AddSqlServerInfrastructure()` quanto `AddMongoDbInfrastructure()` registram `IItemRepository` e `IUnitOfWork`. O último `Add*Infrastructure()` chamado em `Program.cs` sobrescreve os registros anteriores, tornando o comportamento indeterminado quando ambos estão habilitados.

**Ajuste:** Adotar uma estratégia de *feature flag* ou perfil de ambiente:

```csharp
// Program.cs
if (builder.Configuration.GetValue<bool>("UseSqlServer"))
    builder.Services.AddSqlServerInfrastructure(builder.Configuration);

if (builder.Configuration.GetValue<bool>("UseMongoDb"))
    builder.Services.AddMongoDbInfrastructure(builder.Configuration);
```

Ou usar apenas um banco em produção e definir qual será usado via `appsettings`:

```json
"DatabaseProvider": "SqlServer" // ou "MongoDb"
```

```csharp
// Program.cs
var provider = builder.Configuration.GetValue<string>("DatabaseProvider");
switch (provider)
{
    case "SqlServer": builder.Services.AddSqlServerInfrastructure(builder.Configuration); break;
    case "MongoDb":   builder.Services.AddMongoDbInfrastructure(builder.Configuration);   break;
}
```

---

## 2. MongoUnitOfWork não implementa transação real

**Problema:** `MongoUnitOfWork.SaveChangesAsync()` retorna `Task.FromResult(1)` sem executar nenhuma operação real. Em cenários de escrita concorrente ou consistência, isso é problemático.

**Ajuste:** Implementar Unit of Work real usando sessão/client session do MongoDB:

```csharp
public sealed class MongoUnitOfWork : IUnitOfWork
{
    private readonly IClientSessionHandle _session;

    public MongoUnitOfWork(IClientSessionHandle session)
    {
        _session = session;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // O MongoDB não tem contagem de linhas afetadas como SQL.
        // A sessão já mantém as operações em buffer; este método
        // serve como hook para commit explícito se necessário.
        return Task.FromResult(1);
    }
}
```

E no DI:

```csharp
services.AddScoped<IClientSessionHandle>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.StartSession(); // inicia sessão
});
services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
```

---

## 3. JwtSettings.SecretKey exposto no appsettings.json

**Problema:** A chave secreta JWT está hardcoded no `appsettings.json` (`"super-secret-key-used-for-template-only-1234567890"`). Em produção isso é um risco grave de segurança.

**Ajuste:** Remover do `appsettings.json` e usar variáveis de ambiente ou gerenciador de segredos:

```bash
# Windows (dev local)
setx JwtSettings__SecretKey "sua-chave-real-aqui"
```

```json
// appsettings.json — remover a linha SecretKey ou deixar vazia
"JwtSettings": {
  "Issuer": "Fcg.Donations",
  "Audience": "Fcg.Donations.Client",
  "AccessTokenExpirationMinutes": 60
}
```

No `DependencyInjection.cs` da Auth, garantir que a aplicação falhe em startup se a chave não estiver configurada (já existe validação com `throw new InvalidOperationException`).

---

## 4. ExternalQuotesController usa [FromServices] no parâmetro

**Problema:** `ExternalQuotesController.GetZen()` recebe `IExternalQuoteClient` via `[FromServices]` no parâmetro do método, enquanto `ItemsController` recebe dependências pelo construtor. Quebra a padronização.

**Ajuste:** Mover a injeção para o construtor:

```csharp
public sealed class ExternalQuotesController : BaseApiController
{
    private readonly IExternalQuoteClient _externalQuoteClient;

    public ExternalQuotesController(IMediator mediator, IExternalQuoteClient externalQuoteClient)
        : base(mediator)
    {
        _externalQuoteClient = externalQuoteClient;
    }

    [HttpGet("zen")]
    public async Task<IActionResult> GetZen(CancellationToken cancellationToken)
    {
        var quote = await _externalQuoteClient.GetZenAsync(cancellationToken);
        return Ok(ApiResponse<string>.FromSuccess(quote));
    }
}
```

---

## 5. Sem OpenTelemetry exporter configurado

**Problema:** OpenTelemetry está configurado com instrumentação (AspNetCore, HttpClient, Runtime) mas sem nenhum *exporter* — tracing e metrics são gerados mas nunca enviados para lugar algum.

**Ajuste:** Adicionar OTLP exporter no `DependencyInjection.cs` da WebApi:

```csharp
// No AddObservability
services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(options.ServiceName))
    .WithTracing(builder =>
    {
        builder.AddAspNetCoreInstrumentation();
        builder.AddHttpClientInstrumentation();
        builder.AddOtlpExporter(); // envia para collector/grafana/jaeger
    })
    .WithMetrics(builder =>
    {
        builder.AddAspNetCoreInstrumentation();
        builder.AddHttpClientInstrumentation();
        builder.AddRuntimeInstrumentation();
        builder.AddOtlpExporter();
    });
```

Adicionar package reference:

```xml
<PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
```

---

## 6. Cobertura de testes de integração incompleta

**Problema:** Apenas `ItemsController` tem teste de integração. `ExternalQuotesController` não está coberto.

**Ajuste:** Adicionar teste mínimo:

```csharp
public sealed class ExternalQuotesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExternalQuotesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetZen_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/v1/externalquotes/zen");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## 7. KafkaMessagePublisher cria producer por chamada

**Problema:** `KafkaMessagePublisher.PublishAsync()` cria um `ProducerBuilder` e um `producer` novo a cada mensagem publicada. Isso é ineficiente e pode causar contenção de recursos.

**Ajuste:** Tornar o `IProducer<Null, string>` singleton:

```csharp
public sealed class KafkaMessagePublisher : IMessagePublisher
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topicName;
    private readonly ILogger<KafkaMessagePublisher> _logger;

    public KafkaMessagePublisher(IOptions<KafkaSettings> options, ILogger<KafkaMessagePublisher> logger)
    {
        _topicName = options.Value.TopicName;
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(_topicName, new Message<Null, string> { Value = payload }, ct);
        _logger.LogInformation("Published message to topic {TopicName}", _topicName);
    }

    public void Dispose() => _producer.Dispose();
}
```

E no DI:

```csharp
services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
```
