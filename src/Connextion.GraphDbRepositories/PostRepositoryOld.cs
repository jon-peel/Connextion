using Connextion.OldD;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public static class Mapping
{
    public static MiniProfile MiniProfile(IReadOnlyDictionary<string, object> userData)
    {
        var username = userData["username"].As<string>();
        var displayName = userData["displayName"].As<string>();
        var degrees = userData["degrees"].As<byte>();
        return new (username, displayName, degrees);
    }

    public static PostOld Post(IReadOnlyDictionary<string, object> postData)
    {
        var id = Guid.Parse(postData["id"].As<string>());
        var user = MiniProfile(postData["postedBy"].As<IReadOnlyDictionary<string, object>>());
        var postedAt = postData["postedAt"].As<DateTime>();
        var body = postData["body"].As<string>();
        return new (id, user, postedAt, body);
    }

    public static OldD.Profile Profile(IReadOnlyDictionary<string, object> data)
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
        return new OldD.Profile(user, posts, following, followers);
    }
}

public class PostRepositoryOld(ILogger<PostRepositoryOld> logger, IDriver driver) : IPostRepositoryOld

{
    public async Task SubmitBodyAsync(CreatePostCmd body)
    {
        logger.LogInformation("{User} wrote {Body}", body.Username, body.Body);
        var parameters = new
        {
            username = body.Username,
            id = Guid.NewGuid().ToString(),
            body = body.Body,
            postedAt = body.PostedAt
        };
        var (_, result) = await driver
            .ExecutableQuery(
                """
                MATCH (u:User {username:$username})
                CREATE (p:Post {id:$id, body:$body, postedAt:$postedAt})
                CREATE (p)-[:POSTED_BY]->(u);
                """)
            .WithParameters(parameters)
            .ExecuteAsync()
            .ConfigureAwait(false);
        Console.WriteLine(result.ToString());
    }

    public async Task<IReadOnlyList<PostOld>> GetTimelineBodyesAsync(User user)
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
                       { username: postUser.username, displayName: postUser.displayName, degrees: degrees } AS postedBy,
                       post.postedAt AS postedAt,
                       post.body AS body
                """)
            .WithParameters(new { currentUser = user.Username })
            .WithMap(Mapping.Post)
            .ExecuteAsync()
            .ConfigureAwait(false);
        return result;
    }
}