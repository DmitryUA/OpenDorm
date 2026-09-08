using Microsoft.EntityFrameworkCore;
using OpenDorm.API.Handlers;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Infrastructure;
using OpenDorm.Infrastructure.Persistence;
using OpenDorm.Infrastructure.Repositories;
using OpenDorm.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext
builder.Services.AddDbContext<OpenDormDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OpenDormConnectionString")));

// Add services to the container.
var keyBase64 = builder.Configuration["Encryption:Key"]!;
var keyBytes = Convert.FromBase64String(keyBase64);

builder.Services.AddSingleton<IEncryptionService>(new AesGcmEncryptionService(keyBytes));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<OpenDormDbContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<IDormitoryRepository, DormitoryRepository>();
builder.Services.AddScoped<IOccupantRepository, OccupantRepository>();

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssemblyContaining<GetDormitoryListQuery>()); 

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();