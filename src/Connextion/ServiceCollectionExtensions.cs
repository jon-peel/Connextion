using Microsoft.Extensions.DependencyInjection;

namespace Connextion;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddTransient<ProfileService>()
            .AddTransient<PostService>();
}