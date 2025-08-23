using LibraryManagement.DAL;
using LibraryManagement.BLL;
using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;
using LibraryManagement.DAL.Persistance;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Hosting;

namespace LibraryManagement.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog from configuration
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting Library Management System...");
                
                builder.Host.UseSerilog();

                // Add services to the container.
                builder.Services.AddControllersWithViews();

                builder.Services.AddDataAccessLayer(builder.Configuration);
                builder.Services.AddBusinessLogicLayer();

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
                    pattern: "{controller=Books}/{action=Index}/{id?}");

                Log.Information("Library Management System started successfully");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
