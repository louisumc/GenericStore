using GenericStore.Application.Abstractions;
using GenericStore.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace GenericStore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}