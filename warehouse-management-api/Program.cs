using Microsoft.EntityFrameworkCore;
using warehouse_management_api.Endpoints;
using warehouse_management_application;
using warehouse_management_application.Items;
using warehouse_management_application.Shops;
using warehouse_management_application.Storages;
using warehouse_management_core;
using warehouse_management_core.Entities;
using warehouse_management_infrastructure.Db;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext
builder.Services.AddDbContext<WarehouseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor",
        policy => policy.WithOrigins("http://localhost:7113")  // Порт Blazor
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});






// Регистрация репозиториев как Scoped
builder.Services.AddScoped<IRepository<Item>, BasicRepository<Item>>();
builder.Services.AddScoped<IRepository<Storage>, BasicRepository<Storage>>();
builder.Services.AddScoped<IRepository<Shop>, BasicRepository<Shop>>();

// Динамическая регистрация репозиториев для всех сущностей как Scoped
var entityTypes = typeof(IEntity).Assembly
                                    .GetTypes()
                                    .Where(x => !x.IsInterface &&
                                                !x.IsAbstract &&
                                                typeof(IEntity).IsAssignableFrom(x));
foreach (var entityType in entityTypes)
{
    builder.Services
        .AddScoped(typeof(IRepository<>).MakeGenericType(entityType),
                   typeof(BasicRepository<>).MakeGenericType(entityType));
}

// Регистрация сервисов как Scoped
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<ShopService>();

// Регистрация логгеров
builder.Services.AddScoped<ILogger<ShopService>, Logger<ShopService>>();
builder.Services.AddScoped<ILogger<StorageService>, Logger<StorageService>>();
builder.Services.AddScoped<ILogger<ItemService>, Logger<ItemService>>();

// Добавление Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowBlazor"); // Применение политики CORS

// Конфигурация HTTP-пайплайна
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Добавление маршрутов для эндпоинтов
app.MapItemEndpoints();
app.MapStorageEndpoints();
app.MapShopEndpoints();

app.Run();

