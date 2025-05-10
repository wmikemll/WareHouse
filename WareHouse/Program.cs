using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using WareHouse;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";
        options.AccessDeniedPath = "/Home/Index";
    });
// Установка культуры с запятой как разделителем (например, для ru-RU)


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrProcurement", policy =>
        policy.RequireRole("Администратор", "Менеджер по закупкам"));
    options.AddPolicy("AdminOrProcurementOrWareHouseWorker", policy =>
        policy.RequireRole("Администратор", "Менеджер по закупкам", "Кладовщик"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Администратор"));

    options.AddPolicy("AdminSalesManager", policy =>
        policy.RequireRole("Администратор", "Менеджер по продажам"));

    options.AddPolicy("ProcurementManagerPolicy", policy =>
        policy.RequireRole("Менеджер по закупкам"));

    options.AddPolicy("WarehouseWorkerPolicy", policy =>
        policy.RequireRole("Кладовщик"));

    options.AddPolicy("AccountantPolicy", policy =>
        policy.RequireRole("Бухгалтер"));

    options.AddPolicy("FullAccess", policy =>
        policy.RequireRole("Администратор", "Менеджер по продажам", "Менеджер по закупкам", "Кладовщик", "Бухгалтер"));
});
builder.Services.AddDbContext<NeondbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString(("Host=ep-mute-band-a8ch3ky8-pooler.eastus2.azure.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_SBV1YKWrxCf0"))));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();