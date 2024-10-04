using Neo4j.Driver;

namespace Connextion.Graph;

public interface IUserRepository
{
    public Task InitializeUsersAsync();
    public Task<string[]> GetUsernamesAsync();
}

public class UserRepository(IDriver driver) : IUserRepository
{
    public async Task InitializeUsersAsync()
    {
        var userNames = await GetUsernamesAsync().ConfigureAwait(false);
        if (userNames.Length != 0) return;
        await driver
            .ExecutableQuery(
                "CREATE (jonathan:User {userName: 'jonathan', name: 'Jonathan Peel'}), (jack:User {userName: 'jack', name: 'Jack Pool'})")
            .ExecuteAsync();
    }

    public async Task<string[]> GetUsernamesAsync()
    {
        var (queryResults, _) = await driver
            .ExecutableQuery("MATCH (user:User) RETURN user.userName")
            .ExecuteAsync();
        var userNames = queryResults.Select(x => x["user.userName"].As<string>()).ToArray();
        return userNames;
    }
}