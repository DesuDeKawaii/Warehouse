using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NUnit.Framework.Interfaces;
using warehouse_management_application.Items;
using warehouse_management_application.Shops;
using warehouse_management_application.Storages;
using warehouse_management_infrastructure.Db;
using warehouse_management_Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5262/") });

builder.Services.AddScoped<IRepository<Item>, BasicRepository<Item>>();
builder.Services.AddScoped<IRepository<Storage>, BasicRepository<Storage>>();
builder.Services.AddScoped<IRepository<Shop>, BasicRepository<Shop>>();


builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<ShopService>();

builder.Services.AddDbContext<WarehouseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



await builder.Build().RunAsync();
