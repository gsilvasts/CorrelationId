using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.WithCorrelationId()
    .Enrich.WithCorrelationIdHeader()
    .WriteTo.Console(outputTemplate: "[{CorrelationId} - {Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(builder.Logging);

builder.Services.AddHeaderPropagation(options =>
{
    options.Headers.Add("x-correlation-id");
});

builder.Services.AddHttpClient("validate-intermediary", c =>
{
    c.BaseAddress = new Uri("https://localhost:7165");
}).AddHeaderPropagation();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHeaderPropagation();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/request", async (ILogger<Program> logger, IHttpClientFactory clientFactory, bool valid) =>
{
    logger.LogInformation("Starting request {method} with params {@input}", "request", valid);

    var request = new HttpRequestMessage(HttpMethod.Get, "intermediary-request?valid=true");

    var client = clientFactory.CreateClient("validate-intermediary");
    var response = await client.SendAsync(request);

    if (response.IsSuccessStatusCode)
    {
        Log.Information("Retornado válido, respondendo como Ok");
        logger.LogInformation("Ended request {method} with params {@input}", "request", valid);
        return Results.Ok(default);
    }

    Log.Error("Retornado como não válido, respondendo com BadRequest");
    logger.LogInformation("Ended request {method} with params {@input}", "request", valid);

    return Results.BadRequest();
});

app.UseHttpsRedirection();


app.Run();
