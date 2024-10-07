using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class UserRepository(IDriver driver) : IUserRepository
{
    public async Task<User[]> GetUsernamesAsync()
    {
        var (queryResults, _) = await driver
            .ExecutableQuery("MATCH (user:User) RETURN user.username, user.fullName")
            .WithMap(r => new User(r["user.username"].As<string>(), r["user.fullName"].As<string>()))
            .ExecuteAsync();
        return queryResults.ToArray();
    }
}