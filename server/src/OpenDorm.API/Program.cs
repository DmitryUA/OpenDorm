using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenDorm.API.Extensions;
using OpenDorm.API.Handlers;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Abstractions.Search;
using OpenDorm.Application.Common;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Encryption
var keyBase64 = builder.Configuration["Encryption:Key"]!;
var keyBytes = Convert.FromBase64String(keyBase64);
builder.Services.AddSingleton<IEncryptionService>(new AesGcmEncryptionService(keyBytes));

// Search indexes use a separate secret from field encryption.
var searchIndexKeyBase64 = builder.Configuration["SearchIndex:HmacKey"]
                           ?? throw new InvalidOperationException("Search-index HMAC key is not configured.");
var searchIndexKeyBytes = Convert.FromBase64String(searchIndexKeyBase64);
if (searchIndexKeyBytes.Length < 32)
    throw new InvalidOperationException("Search-index HMAC key must be at least 32 bytes.");
if (CryptographicOperations.FixedTimeEquals(keyBytes, searchIndexKeyBytes))
    throw new InvalidOperationException("Search-index HMAC key must be different from the encryption key.");

var occupantSearchIndex = new HmacOccupantSearchIndex(searchIndexKeyBytes);
CryptographicOperations.ZeroMemory(searchIndexKeyBytes);
builder.Services.AddSingleton<IOccupantSearchIndex>(occupantSearchIndex);
builder.Services.AddSingleton<OccupantSearchIndexInterceptor>();
builder.Services.AddScoped<OccupantSearchIndexInitializer>();
builder.Services.AddDbContext<OpenDormDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("OpenDormConnectionString"))
        .AddInterceptors(serviceProvider.GetRequiredService<OccupantSearchIndexInterceptor>()));

// JWT
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JWT settings are not configured.");

// Application Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<OpenDormDbContext>());

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IOccupantRepository, OccupantRepository>();
builder.Services.AddScoped<IDormitoryRepository, DormitoryRepository>();
builder.Services.AddScoped<ISystemFlagRepository, SystemFlagRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),

            // Разрешаем несколько claims "permissions"
            // (по умолчанию ASP.NET Core маппит role в ClaimTypes.Role автоматически)
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Регистрируем политику для каждого permission.
    // Политика требует наличия соответствующего значения в claim "permissions".
    foreach (var permission in RolePermissions.GetAllPermissions())
    {
        options.AddPolicy(permission, policy =>
            policy.RequireClaim("permissions", permission));
    }
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<IApplicationMarker>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<IApplicationMarker>();

builder.Services.AddControllers()
    .AddJsonOptions(options=>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi(options =>
{
    // 1. Добавляем схему Bearer-аутентификации
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Вставьте JWT-токен"
            }
        };
        return Task.CompletedTask;
    });

    // 2. Применяем требование безопасности ко всем операциям
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer")] = []
            }
        ];

        return Task.CompletedTask;
    });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider
        .GetRequiredService<OccupantSearchIndexInitializer>()
        .InitializeAsync();
}

await app.SeedDataAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                    // /openapi/v1.json
    app.MapScalarApiReference();         // /scalar/v1 — красивый UI
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();