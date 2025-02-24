using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// Aguarda o Elasticsearch inicializar
Thread.Sleep(TimeSpan.FromSeconds(30));

// Configuração do Serilog com Elasticsearch
var elasticUri = builder.Configuration["ElasticsearchSettings:Uri"] ?? "http://elasticsearch:9200";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .WriteTo.Elasticsearch(new[] { new Uri(elasticUri) }, opts =>
    {
        opts.DataStream = new DataStreamName("logs", "api-logs", "prod");
        opts.BootstrapMethod = BootstrapMethod.Failure;
    })
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithProperty("Application", "APILoggingElastic")
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}