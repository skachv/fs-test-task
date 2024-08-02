using Common;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Task2.Api.DataGeneration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DbConnectionString")
    ?? throw new ConfigurationErrorsException("Missing connection string.");

builder.Services.AddDbContext<TestTaskContext>(options =>
    options.UseNpgsql(dbConnectionString)
);

builder.Services.AddScoped<DatabaseFiller>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/filldata", (GenerationParameters parameters, DatabaseFiller filler) =>
{
    filler.RefillDatabase(parameters.NumberOfClients, parameters.NumberOfContacts, parameters.RandomSeed);

    return Results.Ok();
})
.WithOpenApi();

app.Run();

class GenerationParameters
{
    public required int NumberOfClients { get; set; }
    public required int NumberOfContacts { get; set; }
    public int? RandomSeed { get; set; }
}
