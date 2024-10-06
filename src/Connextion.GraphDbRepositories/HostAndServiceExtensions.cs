using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public static class HostAndServiceExtensions
{
    public static IServiceCollection AddGraphDbRepositories(this IServiceCollection services) => 
        services
        .AddSingleton<IDriver>(_ => GraphDatabase.Driver("neo4j://neo4j:7687", AuthTokens.Basic("neo4j", "neo4j_pass")))
        .AddScoped<IUserRepository, UserRepository>()
        .AddScoped<IPostRepository, PostRepository>()
        .AddScoped<IProfileRepository, ProfileRepository>();
    
    public static T ConfigureGraphDb<T>(this T host) where T : IHost
    {
        using var scope = host.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        userRepository.InitializeUsersAsync().GetAwaiter().GetResult();
        return host;
    }
}