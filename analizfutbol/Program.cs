using Microsoft.Extensions.Options;
using MongoDB.Driver;
using analizfutbol.Services;
using analizfutbol.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB ayarlarını düzgün şekilde yapılandır
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// MongoDB servisini ekle
builder.Services.AddSingleton<IMongoDBService, MongoDBService>();

// MongoDB ayarlarını kontrol et
var mongoSettings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
if (string.IsNullOrEmpty(mongoSettings?.ConnectionString))
{
    throw new Exception("MongoDB connection string is missing or empty in configuration!");
}
Console.WriteLine($"MongoDB Connection String: {mongoSettings.ConnectionString}");
Console.WriteLine($"MongoDB Database Name: {mongoSettings.DatabaseName}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Uygulama başlatılıyor...");
app.Run();
