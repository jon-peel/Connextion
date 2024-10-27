using Connextion.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public static class HostAndServiceExtensions
{
    public static IServiceCollection AddGraphDbRepositories(this IServiceCollection services) =>
        services
            .AddSingleton(DriverFactory)
            .AddTransient<IEventRepository, EventRepository>()
            .AddTransient<IProfileRepository, ProfileRepository>()
            .AddTransient<UserRepository>()
            .AddTransient<IUserRepository, UserRepository>()
            .AddTransient<IPostRepository, PostRepository>()
            .AddTransient<IMessageRepository, MessageRepository>()
            .AddTransient<ConfigureTheDatabase>();

    public static T ConfigureGraphDb<T>(this T host) where T : IHost
    {
        using var scope = host.Services.CreateScope();
        var configureTheDatabase = scope.ServiceProvider.GetRequiredService<ConfigureTheDatabase>();
        configureTheDatabase.RunAsync().GetAwaiter().GetResult();
        return host;
    }

    static IDriver DriverFactory(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var uri = config["Neo4j:Uri"];
        var user = config["Neo4j:User"];
        var pass = config["Neo4j:Password"];
        return GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass));
    }

}