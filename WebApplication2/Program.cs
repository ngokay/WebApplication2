using CardPassportApi.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Diagnostics;
using WebApplication2.Data;
using WebApplication2.Implement;
using WebApplication2.Interface;

var builder = WebApplication.CreateBuilder(args);

// config 
var configValue = builder.Configuration.GetValue<string>("Postgress:Connection");
var configRedis = builder.Configuration.GetValue<string>("Redis:Connection");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// add DI
builder.Services.AddDbContext<AppDbContext>(options =>
       options.UseNpgsql(configValue));
// add DI Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configRedis!));
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<ITracingService, TracingService>();

// test cicd 1

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(serviceName: "sample-net-app")
        )
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddSource("my-dotnet-service")
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri("http://localhost:4317");

            otlpOptions.Protocol = OtlpExportProtocol.Grpc;
        }))
    .WithLogging(logging =>
    {
        logging.AddConsoleExporter();
        logging.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri("http://localhost:4317");

            otlpOptions.Protocol = OtlpExportProtocol.Grpc;
        });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("MyCustomMetrics"); // ✅ Meter cho metric thủ công
        metrics.AddRuntimeInstrumentation(); // CPU, memory...
        metrics.AddAspNetCoreInstrumentation(); // Request count, duration...
        metrics.AddHttpClientInstrumentation(); // Outgoing requests
        metrics.AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri("http://localhost:4317");

            opt.Protocol = OtlpExportProtocol.Grpc;
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TracingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => $"Hello World! OpenTelemetry Trace: {Activity.Current?.Id}");

app.Run();
