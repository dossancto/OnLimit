using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OnLimit;
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
            Limit = new()
            {
                Tokens = 1000,
                Users = 10,
                CanUse = false
            }
          },

          new("PREMIUM")
          {
            Limit = new()
            {
                Tokens = 5000,
                CanUse = true,
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

app.MapGet("/set-plan", async (
      [FromQuery] string plan,
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    await usageManager.SetPlan("123", plan);

    return "ok";
});

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
    var actualUsers = 123;

    await usageManager.Usage("123", [
        new(x => x.Users, Count: 500, Used: actualUsers),
        new(x => x.Tokens, 500),
        new(x => x.CanUse),
    ]);

    await usageManager.Consume("123", [
        new(x => x.Tokens, 500)
    ]);

    return "ok";
});

app.Run();

class MyPlan
{
    public long Users { get; set; }

    [IncrementalUsageLimit]
    public long Tokens { get; set; }

    [UsageSwitch]
    public bool CanUse { get; set; }
}
