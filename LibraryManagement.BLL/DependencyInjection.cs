

using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddScoped<IBookService,BookService>();
        services.AddScoped<IImageService, ImageService>();

        return services;
    }

}
