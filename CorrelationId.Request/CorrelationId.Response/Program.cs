using CorrelationId.Response;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.WithCorrelationId()
    .Enrich.WithCorrelationIdHeader("x-correlation-id")
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{CorrelationId} - {Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(builder.Logging);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/validate", (ILogger<Program> logger, bool valid) =>
    {
        logger.LogInformation("Starting request {method} with params {@input}", "Response", valid);
        var result = Validate.Validated(valid);
        logger.LogInformation("Ended request {method} with params {@input}", "Response", valid);

        if (result)
        {   
            return Results.Ok(default);
        }
        else
        {            
            return Results.BadRequest();
        }
    }).WithName("GetValidate")
    .WithOpenApi();

app.Run();

