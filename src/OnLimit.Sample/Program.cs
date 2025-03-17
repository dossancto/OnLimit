using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OnLimit;
using OnLimit.DependencyInjection;
using OnLimit.FieldConfigs;
using OnLimit.Interfaces;
using OnLimit.Postgres;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNpgsqlDataSource("Server=localhost;Port=5432;Database=onlimit;User Id=postgres;Password=postgres;");

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
                Money = new(0, 100),
                Users = 10,
                CanUse = true
            }
          },

          new("PREMIUM")
          {
            Limit = new()
            {
                Tokens = 5000,
                Money = new(0, 200),
                CanUse = true,
                Users = 10
            }
          }
        ]

    })
    .AddPostgres()
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

app.MapGet("/actual", async (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    var res = await usageManager.GetActualPlan("123");

    if (res is null)
    {
        return Results.NoContent();
    }

    var formatedResult = new
    {
        res.Plan,
        res.CreatedAt,
    };

    return Results.Ok(formatedResult);
});


app.MapGet("/used", (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    return usageManager.GetConsumition("123");
});

app.MapGet("/plans", (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    return usageManager.ListPlans();
});

app.MapGet("/consumed", async (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    var actualUsers = 123;

    var res = await usageManager.Usage("123", [
        new(x => x.Users, Count: 1, Used: actualUsers),
        new(x => x.Tokens, 500),
        new(x => x.CanUse),
    ]);

    if (res is not null)
    {
        return Results.BadRequest(res);
    }

    await usageManager.Consume("123", [
        new(x => x.Tokens, -500)
    ]);

    return Results.Ok(new { message = "ok" });
});

app.MapGet("/consume", async (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    var res = await usageManager.Usage("123", [
        new(x => x.Users, Count: 1),
        new(x => x.Tokens, 500),
        new(x => x.Money, 33),
        new(x => x.CanUse),
    ]);

    if (res is not null)
    {
        return Results.BadRequest(res);
    }

    await usageManager.Consume("123", [
        new(x => x.Money, 33),
        // new(x => x.Users, 1),
    ]);

    return Results.Ok(new { message = "ok" });
});

app.Run();

class MyPlan
{
    [IncrementalUsageLimit]
    public RangedField Money { get; set; } = new();

    public long Users { get; set; }

    [IncrementalUsageLimit]
    public long Tokens { get; set; }

    [UsageSwitch]
    public bool CanUse { get; set; }
}
