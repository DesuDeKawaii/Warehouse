using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using warehouse_management_application.Items;
using warehouse_management_application.Shops;
using warehouse_management_application.Storages;
using warehouse_management_infrastructure.Db;
using warehouse_management_Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IRepository<Item>, BasicRepository<Item>>();
builder.Services.AddScoped<IRepository<Storage>, BasicRepository<Storage>>();
builder.Services.AddScoped<IRepository<Shop>, BasicRepository<Shop>>();



builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<ShopService>();



await builder.Build().RunAsync();
