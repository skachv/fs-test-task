using Common;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Task1.DataAccess;
using Task1.Domain;
using Task1.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure logging
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    // Logger registrations.
    var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DbConnectionString")
    ?? throw new ConfigurationErrorsException("Missing connection string.");

    IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
    {
        { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
        { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
        { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
        { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
        { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
        { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
        { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
        { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
    };

    builder.Services.AddSerilog(options =>
        options.WriteTo.PostgreSQL(dbConnectionString, "Logs", columnWriters, needAutoCreateTable: true)
    );

    builder.Services.AddHttpLogging(logging =>
    {
        logging.LoggingFields = HttpLoggingFields.All;
        logging.RequestHeaders.Add("sec-ch-ua");
        logging.ResponseHeaders.Add("MyResponseHeader");
        logging.MediaTypeOptions.AddText("application/javascript");
        logging.RequestBodyLogLimit = 4096;
        logging.ResponseBodyLogLimit = 4096;
        logging.CombineLogs = true;
    });

    // Data access registrations
    builder.Services.AddDbContext<TestTaskContext>(options =>
        options.UseNpgsql(dbConnectionString)
    );
    builder.Services.AddScoped<IRecordRepository, RecordRepository>();

    // Buisness logic registrations
    builder.Services.AddScoped<RecordSaver>();
    builder.Services.AddScoped<RecordFinder>();

    var app = builder.Build();

    // Init Database before logger does it
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetService<TestTaskContext>();
        if (context != null)
        {
            context.Database.EnsureCreated();
        }
    }

    app.UseHttpLogging(); // This one writes bodies

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.MapPut("/", async (Dictionary<int, string?>[] records, RecordSaver saver) =>
    {
        if (records.Any(x => x.Count > 1 || x.Count == 0))
        {
            return Results.BadRequest("Wrong format of array element.");
        }

        try
        {
            return Results.Ok(await saver.SaveItems(records.SelectMany(dict => dict).ToArray()));
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    })
    .WithOpenApi();

    app.MapPost("/", async (RecordSearchQuery? query, RecordFinder finder) =>
    {
        try
        { 
            return Results.Ok(await finder.Find(query));
        }
        catch(ValidationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    })
    .WithOpenApi();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal($"Application terminated unexpectedly with message: {ex.Message}");
}
finally
{
    Log.CloseAndFlush();
}