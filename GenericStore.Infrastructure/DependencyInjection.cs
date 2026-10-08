using GenericStore.Application.Abstractions;
using GenericStore.Application.Settings;
using GenericStore.Infrastructure.Persistence;
using GenericStore.Infrastructure.Repositories;
using GenericStore.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace GenericStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        services.AddDbContext<GenericStoreDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}