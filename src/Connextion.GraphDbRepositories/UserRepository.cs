using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class UserRepository(IDriver driver) : IUserRepository
{
    public async Task InitializeUsersAsync()
    {
        var userNames = await GetUsernamesAsync().ConfigureAwait(false);
        if (userNames.Length != 0) return;
        await driver
            .ExecutableQuery(
                "CREATE (jonathan:User {userName: 'jonathan', fullName: 'Jonathan Peel'}), (jack:User {userName: 'jack', fullName: 'Jack Pool'})")
            .ExecuteAsync();
    }

    public async Task<User[]> GetUsernamesAsync()
    {
        var (queryResults, _) = await driver
            .ExecutableQuery("MATCH (user:User) RETURN user.userName, user.fullName")
            .WithMap(r => new User(r["user.userName"].As<string>(), r["user.fullName"].As<string>()))
            .ExecuteAsync();
        return queryResults.ToArray();
    }
}