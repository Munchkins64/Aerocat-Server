// In project: Aerocat.Server
// File: Program.cs
using Aerocat.Server.Data;
using Aerocat.Server.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllersWithViews();

// 2. Add Database Context for SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=aerocat.db";
builder.Services.AddDbContext<AerocatDbContext>(options =>
    options.UseSqlite(connectionString));

// 3. Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 4. Map the SignalR Hub endpoint
app.MapHub<ChatHub>("/chathub");

app.Run();