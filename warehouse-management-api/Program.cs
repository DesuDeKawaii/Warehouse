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

// ����������� DbContext
//builder.Services.AddDbContext<WarehouseContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<FakeContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor",
        policy => policy//.WithOrigins("http://localhost:7113")  // ���� Blazor
                        .SetIsOriginAllowed(x=> true)  
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});






//// ����������� ������������ ��� Scoped
//builder.Services.AddScoped<IRepository<Item>, FakeRepository<Item>>();
//builder.Services.AddScoped<IRepository<Storage>, FakeRepository<Storage>>();
//builder.Services.AddScoped<IRepository<Shop>, FakeRepository<Shop>>();

// ������������ ����������� ������������ ��� ���� ��������� ��� Scoped
var entityTypes = typeof(IEntity).Assembly
                                    .GetTypes()
                                    .Where(x => !x.IsInterface &&
                                                !x.IsAbstract &&
                                                typeof(IEntity).IsAssignableFrom(x));

foreach (var entityType in entityTypes)
{
    builder.Services
        .AddScoped(typeof(IRepository<>).MakeGenericType(entityType),
                   typeof(FakeRepository<>).MakeGenericType(entityType));
}

// ����������� �������� ��� Scoped
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<ShopService>();

// ����������� ��������
builder.Services.AddScoped<ILogger<ShopService>, Logger<ShopService>>();
builder.Services.AddScoped<ILogger<StorageService>, Logger<StorageService>>();
builder.Services.AddScoped<ILogger<ItemService>, Logger<ItemService>>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



// ���������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowAll");
app.UseCors("AllowBlazor"); // ���������� �������� CORS

// ������������ HTTP-���������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ���������� ��������� ��� ����������
app.MapItemEndpoints();
app.MapStorageEndpoints();
app.MapShopEndpoints();

app.Run();

