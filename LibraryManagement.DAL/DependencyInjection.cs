
using LibraryManagement.DAL.Interfaces;
using LibraryManagement.DAL.Persistance;
using LibraryManagement.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceTracker.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddDbContext<LibraryDbContext>(options =>
             options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));



        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        

        return services;
    }
}
