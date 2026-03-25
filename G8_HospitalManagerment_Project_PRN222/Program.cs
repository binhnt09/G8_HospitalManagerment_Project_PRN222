using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Repository;
using G8_HospitalManagerment_Project_PRN222.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DbHospitalManagementContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MyCnn")));

// đăng ký repository and service
builder.Services.AddScoped<ILabOrderRepository, LabOrderRepository>();
builder.Services.AddScoped<ILabOrderService, LabOrderService>();

builder.Services.AddSession();


builder.Services.AddScoped<AppointmentRepository>();
builder.Services.AddScoped<AppointmentService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
