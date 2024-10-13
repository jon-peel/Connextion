using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class UserRepository(IDriver driver) : RepositoryBase(driver), IUserRepository
{
    public IAsyncEnumerable<ProfileSummary> GetAllUsersAsync()
    {
        const string query = 
            """
            MATCH (user:User:Profile) 
            RETURN user.id AS id, 
                   user.displayName AS displayName
            """;
        return ExecuteReaderQueryAsync(query, new { }, MapProfileSummary);
    }

    public Task<Result> CreateUserAsync(CreateUserCmd cmd)
    {
        const string query =
            "CREATE (u:User:Profile { id: $username, username: $username, displayName: $displayName })";
        var parameters = new { username = cmd.Username, displayName = cmd.DisplayName };
        return ExecuteWriteAsync(query, parameters);
    }

    public async Task<User> GetUserAsync(string id)
    {
        const string query =
            """
            MATCH (profile:Profile { id: $id })
            RETURN profile.id AS id,
                   profile.displayName as displayName
            """;
        var profiles = await ExecuteQueryAsync(query, new { id }, MapProfile).ConfigureAwait(false);
        return profiles.Single();
    }
    
    User MapProfile(IRecord record)
    {
        var id = new ProfileId(record["id"].As<string>());
        var displayName = new DisplayName(record["displayName"].As<string>());
        var posts = GetPostsAsync(id.Value);
        var following = GetFollowingAsync(id.Value);
        var followers = GetFollowersAsync(id.Value);
        return new(id, displayName, posts, following, followers);
    }
    
    internal ProfileSummary MapProfileSummary(IReadOnlyDictionary<string, object> record)
    {
        var id = record["id"].As<string>();
        var displayName = record["displayName"].As<string>();
        return new(new(id), new(displayName), to => GetDegreesFromAsync(id, to.Value));
    }
    
    async Task<byte> GetDegreesFromAsync(string id, string to)
    {
        if (id == to) return 0;
        const string query =
            """
            MATCH (from: Profile { id: $id })
            MATCH (to: Profile { id: $id })
            RETURN length(shortestPath((from)-[*]-(to))) AS degrees
            """;
        var results = await ExecuteQueryAsync(query, new { id, to }, x => x["degrees"].As<byte>())
            .ConfigureAwait(false);
        return results.Single();
    }
    
    IAsyncEnumerable<Post> GetPostsAsync(string id)
    {
        const string query =
            """
            MATCH (profile:Profile { id: $id })
            MATCH (profile)<-[:POSTED_BY]-(post:Post)
            ORDER BY post.postedAt DESC 
            RETURN post.id AS id,
                   post.postedAt AS postedAt,
                   post.body AS body
            """;
        return ExecuteReaderQueryAsync(query, new { id }, MapPost);
    }
    
    static Post MapPost(IRecord record)
    {
        var id = new PostId(record["id"].As<string>());
        var postedAt = record["postedAt"].As<DateTime>();
        var body = new PostBody(record["body"].As<string>());
        return new(id, postedAt, body);
    }

    IAsyncEnumerable<ProfileSummary> GetFollowingAsync(string id)
    {
        const string query =
            """
            MATCH (p:Profile { id: $id })-[:FOLLOWS]->(Profile)
            RETURN p.id as id,
                   p.displayName as displayName
            """;
        return ExecuteReaderQueryAsync(query, new { id }, MapProfileSummary);
    }

    IAsyncEnumerable<ProfileSummary> GetFollowersAsync(string id)
    {
        const string query =
            """
            MATCH (p:Profile { id: $id })<-[:FOLLOWS]-(Profile)
            RETURN p.id as id,
                   p.displayName as displayName
            """;
        return ExecuteReaderQueryAsync(query, new { id }, MapProfileSummary);
    }
}