using System.Runtime.CompilerServices;
using Connextion.Events;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Connextion.GraphDbRepositories")]
namespace Connextion;


public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddTransient<EventService>()
            .AddTransient<MessageService>()
            .AddTransient<ProfileService>()
            .AddTransient<PostService>();
}