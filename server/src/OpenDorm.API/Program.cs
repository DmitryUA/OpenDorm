using Microsoft.EntityFrameworkCore;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Abstractions;
using OpenDorm.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext
builder.Services.AddDbContext<OpenDormDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OpenDormConnectionString")));

// Add services to the container.
var keyBase64 = builder.Configuration["Encryption:Key"]!;
var keyBytes = Convert.FromBase64String(keyBase64);

builder.Services.AddSingleton<IEncryptionService>(new AesGcmEncryptionService(keyBytes));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();