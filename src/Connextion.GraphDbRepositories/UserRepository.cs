using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class UserRepository(IDriver driver, ProfileRepository profileRepository) : RepositoryBase(driver), IUserRepository
{
    public IAsyncEnumerable<ProfileSummary> GetAllUsersAsync()
    {
        const string query = 
            """
            MATCH (user:User:Profile) 
            RETURN user.id AS id, 
                   user.displayName AS displayName
            """;
        return ExecuteReaderQueryAsync(query, new { }, profileRepository.MapProfileSummary);
    }

    public Task<Result> CreateUserAsync(CreateUserCmd cmd)
    {
        const string query =
            "CREATE (u:User:Profile { id: $username, username: $username, displayName: $displayName })";
        var parameters = new { username = cmd.Username, displayName = cmd.DisplayName };
        return ExecuteWriteAsync(query, parameters);
    }
}