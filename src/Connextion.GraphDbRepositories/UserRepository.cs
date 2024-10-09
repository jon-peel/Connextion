using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class UserRepository(ILogger<UserRepository> logger, IDriver driver) : IUserRepository
{
    public async Task<User[]> GetUsernamesAsync()
    {
        var (queryResults, _) = await driver
            .ExecutableQuery("MATCH (user:User) RETURN user.username, user.fullName")
            .WithMap(r => new User(r["user.username"].As<string>(), r["user.fullName"].As<string>()))
            .ExecuteAsync();
        return queryResults.ToArray();
    }

    public Task CreateUserAsync(CreateUserCmd cmd)
    {
        logger.LogInformation("Creating user {FullName} with username {Username}", cmd.FullName, cmd.Username);
        return driver
            .ExecutableQuery(@"CREATE (u:User {username: $username, fullName: $fullName})")
            .WithParameters(new { username = cmd.Username, fullName = cmd.FullName })
            .ExecuteAsync();
    }
}