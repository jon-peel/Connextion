using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public static class Mapping
{
    public static User User(IReadOnlyDictionary<string, object> userData)
    {
        var userName = userData["userName"].As<string>();
        var fullName = userData["fullName"].As<string>();
        return new User(userName, fullName);
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
        var fullName = data["fullName"].As<string>();
        var posts = data["posts"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(Post)
            .ToArray();
        return new Profile(fullName, posts, []);
    }
}


public class PostRepository(IDriver driver) : IPostRepository
{
    public async Task SubmitStatusAsync(CreatePostCmd status)
    {
        var parameters = new
        {
            userName = status.UserName,
            id = Guid.NewGuid().ToString(),
            status = status.Status,
            postedAt = DateTime.Now,
        };
        var (_, result) = await driver
            .ExecutableQuery(
                """
                MATCH (u:User {userName:$userName})
                CREATE (p:Post {id:$id, status:$status, postedAt:$postedAt})
                CREATE (p)-[:POSTED_BY]->(u);
                """)
            .WithParameters(parameters)
            .ExecuteAsync()
            .ConfigureAwait(false);
        Console.WriteLine(result.ToString());
    }

    public async Task<IReadOnlyList<Post>> GetTimelineStatusesAsync(string userName)
    {
        var (result, _) = await driver
            .ExecutableQuery(
                """
                MATCH (p:Post)-[:POSTED_BY]->(u:User)
                RETURN 
                    p.id AS id,
                    { userName: u.userName, fullName: u.fullName } AS postedBy,
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