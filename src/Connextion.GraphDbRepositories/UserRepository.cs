using Connextion.OldD;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class UserRepository(ILogger<UserRepository> logger, IDriver driver) : IUserRepository
{
    public async Task<User[]> GetAllUsersAsync()
    {
        var (queryResults, _) = await driver
            .ExecutableQuery("MATCH (user:User) RETURN user.username AS username, user.fullName AS fullName, 0 AS degrees")
            .WithMap(Mapping.MiniProfile)
            .ExecuteAsync();
        return queryResults.ToArray();
    }

    public Task CreateUserAsync(CreateUserCmd cmd)
    {
        logger.LogInformation("Creating user profile {FullDisplayName} with username {Username}", cmd.DisplayName, cmd.Username);
        return driver
            .ExecutableQuery(@"CREATE (u:User:Profile { id: $username, username: $username, displayName: $displayName })")
            .WithParameters(new { username = cmd.Username, displayName = cmd.DisplayName })
            .ExecuteAsync();
    }
}