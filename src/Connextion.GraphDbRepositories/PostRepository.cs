using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public static class Mapping
{
    public static User User(IReadOnlyDictionary<string, object> userData)
    {
        var username = userData["username"].As<string>();
        var fullName = userData["fullName"].As<string>();
        return new User(username, fullName);
    }

    public static Post Post(IReadOnlyDictionary<string, object> postData)
    {
        var id = Guid.Parse(postData["id"].As<string>());
        var user = User(postData["postedBy"].As<IReadOnlyDictionary<string, object>>());
        var postedAt = postData["postedAt"].As<DateTime>();
        var status = postData["status"].As<string>(); 
        return new Post(id, user, postedAt, status);
    }

    public static Profile Profile(IReadOnlyDictionary<string, object> data)
    {
        var user = User(data["user"].As<IReadOnlyDictionary<string, object>>());
        var posts = data["posts"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(Post)
            .ToArray();
        var following = data["following"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(User)
            .ToArray();
        var followers = data["followers"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(User)
            .ToArray();
        return new Profile(user, posts, following, followers);
    }
}


public class PostRepository(IDriver driver) : IPostRepository
{
    public async Task SubmitStatusAsync(CreatePostCmd status)
    {
        var parameters = new
        {
            username = status.Username,
            id = Guid.NewGuid().ToString(),
            status = status.Status,
            postedAt = DateTime.Now
        };
        var (_, result) = await driver
            .ExecutableQuery(
                """
                MATCH (u:User {username:$username})
                CREATE (p:Post {id:$id, status:$status, postedAt:$postedAt})
                CREATE (p)-[:POSTED_BY]->(u);
                """)
            .WithParameters(parameters)
            .ExecuteAsync()
            .ConfigureAwait(false);
        Console.WriteLine(result.ToString());
    }

    public async Task<IReadOnlyList<Post>> GetTimelineStatusesAsync(string username)
    {
        var (result, _) = await driver
            .ExecutableQuery(
                """
                MATCH (p:Post)-[:POSTED_BY]->(u:User)
                RETURN 
                    p.id AS id,
                    { username: u.username, fullName: u.fullName } AS postedBy,
                    p.postedAt AS postedAt,
                    p.status AS status
                ORDER BY p.postedAt DESC
                """)
            .WithMap(Mapping.Post)
            .ExecuteAsync()
            .ConfigureAwait(false);
        return result;
    }

 
}