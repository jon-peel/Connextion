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
            .AddScoped<IProfileRepository, ProfileRepository>()
            .AddScoped<UserRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IPostRepository, PostRepository>()
            .AddTransient<IMessageRepository, MessageRepository>()
            .AddScoped<ConfigureTheDatabase>();

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