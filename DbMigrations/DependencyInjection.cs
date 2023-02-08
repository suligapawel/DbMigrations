using Microsoft.Extensions.DependencyInjection;

namespace DbMigrations;

public static class DependencyInjection
{
    public static IServiceCollection AddDbMigrations(this IServiceCollection services)
        => services;

}