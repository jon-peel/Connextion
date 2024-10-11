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
        logger.LogInformation("Creating user {FullName} with username {Username}", cmd.FullName, cmd.Username);
        return driver
            .ExecutableQuery(@"CREATE (u:User {username: $username, fullName: $fullName})")
            .WithParameters(new { username = cmd.Username, fullName = cmd.FullName })
            .ExecuteAsync();
    }
}