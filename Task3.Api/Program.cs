using Common;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Task3.Api;

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

builder.Services.AddScoped<DatesGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/filldata", (GenerationParameters parameters, DatesGenerator generator) =>
{
    generator.FillData(parameters.NumberOfClients, parameters.NumberOfDates, parameters.RandomSeed);
    return Results.Ok();
})
.WithOpenApi();

app.Run();

class GenerationParameters
{
    public required int NumberOfClients { get; set; }
    public required int NumberOfDates { get; set; }
    public int? RandomSeed { get; set; }
}