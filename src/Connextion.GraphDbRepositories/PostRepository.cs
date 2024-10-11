using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public static class Mapping
{
    public static MiniProfile MiniProfile(IReadOnlyDictionary<string, object> userData)
    {
        var username = userData["username"].As<string>();
        var fullName = userData["fullName"].As<string>();
        var degrees = userData["degrees"].As<byte>();
        return new (username, fullName, degrees);
    }

    public static Post Post(IReadOnlyDictionary<string, object> postData)
    {
        var id = Guid.Parse(postData["id"].As<string>());
        var user = MiniProfile(postData["postedBy"].As<IReadOnlyDictionary<string, object>>());
        var postedAt = postData["postedAt"].As<DateTime>();
        var status = postData["status"].As<string>();
        return new (id, user, postedAt, status);
    }

    public static Profile Profile(IReadOnlyDictionary<string, object> data)
    {
        var user = MiniProfile(data["user"].As<IReadOnlyDictionary<string, object>>());
        var posts = data["posts"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(Post)
            .ToArray();
        var following = data["following"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(MiniProfile)
            .ToArray();
        var followers = data["followers"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(MiniProfile)
            .ToArray();
        return new Profile(user, posts, following, followers);
    }
}

public class PostRepository(ILogger<PostRepository> logger, IDriver driver) : IPostRepository
{
    public async Task SubmitStatusAsync(CreatePostCmd status)
    {
        logger.LogInformation("{User} wrote {Status}", status.Username, status.Status);
        var parameters = new
        {
            username = status.Username,
            id = Guid.NewGuid().ToString(),
            status = status.Status,
            postedAt = status.PostedAt
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

    public async Task<IReadOnlyList<Post>> GetTimelineStatusesAsync(CurrentUser user)
    {
        var (result, _) = await driver
            .ExecutableQuery(
                """
                MATCH (currentUser:User { username: $currentUser })
                MATCH (postUser:User)
                WITH currentUser,
                     postUser, 
                     CASE WHEN currentUser = postUser THEN 0
                     ELSE length(shortestPath((postUser)-[*]-(currentUser)))
                     END AS degrees
                WHERE degrees <= 2
                MATCH (post:Post)-[:POSTED_BY]->(postUser)
                ORDER by post.postedAt DESC
                RETURN post.id AS id,
                       { username: postUser.username, fullName: postUser.fullName, degrees: degrees } AS postedBy,
                       post.postedAt AS postedAt,
                       post.status AS status
                """)
            .WithParameters(new { currentUser = user.Username })
            .WithMap(Mapping.Post)
            .ExecuteAsync()
            .ConfigureAwait(false);
        return result;
    }
}