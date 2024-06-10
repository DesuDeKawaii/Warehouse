using Microsoft.EntityFrameworkCore;
using warehouse_management_application;
using warehouse_management_core;
using warehouse_management_infrastructure.Db;

var builder = WebApplication.CreateBuilder(args);

var entityTypes = typeof(IEntity).Assembly
                                        .GetTypes()
                                        .Where(x => !x.IsInterface &&
                                                    !x.IsAbstract &&
                                                    typeof(IEntity).IsAssignableFrom(x));
foreach (var entityType in entityTypes)
    builder.Services
        .AddSingleton(typeof(BasicRepository<>).MakeGenericType(entityType),
                      typeof(BasicRepository<>).MakeGenericType(entityType));
foreach (var serviceType in typeof(IServise).Assembly
                                            .GetTypes()
                                            .Where(x => !x.IsInterface && !x.IsAbstract &&
                                                    typeof(IEntity).IsAssignableFrom(x)))
    builder.Services.AddSingleton(serviceType);


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

app.MapGet("", () => { }).ExcludeFromDescription();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
