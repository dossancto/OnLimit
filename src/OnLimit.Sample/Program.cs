using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OnLimit.DependencyInjection;
using OnLimit.Interfaces;
using OnLimit.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var clientSettings = MongoClientSettings.FromUrl(new("mongodb://admin:examplepassword@localhost:27017/"));

var mongoClient = new MongoClient(clientSettings);

builder.Services.AddSingleton<IMongoClient>(_ => mongoClient);

builder.Services
    .AddOnLimit<MyPlan>(new()
    {
        Values = [
          new("FREE")
          {
          // TODO: Remove to "Limit"
            Value = new()
            {
                Users = 10
            }
          }
        ]

    })
    .AddMongoDB()
;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/plans", (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    return usageManager.ListPlans();
});

app.MapGet("/consume", async (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    await usageManager.Usage("123", [
        new(x => x.CanUse, 0)
    ]);

    return "ok";
});

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class MyPlan
{
    public long Users { get; set; }
    public bool CanUse { get; set; }
}
