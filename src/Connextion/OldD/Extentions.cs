using Microsoft.Extensions.DependencyInjection;

namespace Connextion.OldD;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddTransient<ProfileService>();
}