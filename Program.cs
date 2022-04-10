using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Data.Sqlite;
using Dapper;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "public"
});

var connectionString = builder.Configuration.GetConnectionString("QuoteDb") ?? "Data Source=quotes.db";
builder.Services.AddScoped(_ => new SqliteConnection(connectionString));

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.IncludeFields = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await EnsureDb(app.Services, app.Logger);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseFileServer();


app.MapGet("/quotes", async (SqliteConnection db) =>
    await db.QueryAsync<Quote>("SELECT * FROM Quotes"))
    .WithName("GetQuotes");

app.MapGet("/quote/{id}", async (int id, SqliteConnection db) =>
    await db.QuerySingleOrDefaultAsync<Quote>("SELECT * FROM Quotes WHERE Id = @id", new { id })
        is Quote quote
            ? Results.Ok(quote)
            : Results.NotFound())
    .WithName("GetQuoteById")
    .Produces<Quote>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapPost("/quotes", async (Quote quote, SqliteConnection db) =>
    {
        var newTodo = await db.QuerySingleAsync<Quote>(
            "INSERT INTO Quotes(Text, Author, Date) Values(@Text, @Author, @Date) RETURNING * ", quote);

        return Results.Created($"/quote/{newTodo.Id}", newTodo);
    })
    .WithName("CreateQuote")
    .Produces<Quote>(StatusCodes.Status201Created);

app.Run();

async Task EnsureDb(IServiceProvider services, ILogger logger)
{
    logger.LogInformation("Ensuring database exists at connection string '{connectionString}'", connectionString);

    using var db = services.CreateScope().ServiceProvider.GetRequiredService<SqliteConnection>();
    var sql = $@"CREATE TABLE IF NOT EXISTS Quotes (
                  {nameof(Quote.Id)} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                  {nameof(Quote.Text)} TEXT NOT NULL,
                  {nameof(Quote.Author)} TEXT NOT NULL,
                  {nameof(Quote.Date)} TEXT NOT NULL,
                  {nameof(Quote.IsFeatured)} INTEGER DEFAULT 0 NOT NULL CHECK({nameof(Quote.IsFeatured)} IN (0, 1))
                 );";
    await db.ExecuteAsync(sql);
}

public class Quote
{
    public int Id { get; set; }
    [Required]
    public string? Text { get; set; }
    public string? Author { get; set; }
    public DateTime Date { get; set; }
    public bool IsFeatured { get; set; }
}