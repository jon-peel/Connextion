using Connextion.OldD;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public abstract class RepositoryBase(IDriver driver)
{
    protected readonly IDriver Driver = driver;
    
    protected async IAsyncEnumerable<TOut> ExecuteReaderQueryAsync<TOut>(string query, object parameters, Func<IRecord, TOut> map)
    {
        await using var session = Driver.AsyncSession();
        var reader = await session.RunAsync(query, parameters).ConfigureAwait(false);
        while (await reader.FetchAsync())
        {
            var result = map(reader.Current);
            yield return result;
        }
    }
}

public class PostRepository(IDriver driver) : RepositoryBase(driver), IPostRepository
{
    // readonly IDriver _driver = driver;

    public async Task<Result> CreatePostAsync(PostCreated cmd)
    {
        const string query =
            """
            MATCH (profile:Profile { id: $profileId })
            CREATE (post:Post { id: $postId, body: $body, postedAt: $postedAt })
            CREATE (post)-[:POSTED_BY]->(profile)
            """;
        try
        {
            await Driver
                .ExecutableQuery(query)
                .WithParameters(new
                {
                    profileId = cmd.CreatedBy.Value,
                    postId = cmd.Id.Value.ToString(),
                    body = cmd.Body.Value,
                    postedAt = cmd.PostedAt
                })
                .ExecuteAsync().ConfigureAwait(false);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Error(e.Message);
        }
    }

    public  IAsyncEnumerable<TimeLinePostDto> GetTimeLineAsync(ProfileId currentUserId)
    {
        const string query =
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
            """;
        return ExecuteReaderQueryAsync(query, new { currentUser = currentUserId.Value }, MapTimeLinePost);
    }

    TimeLinePostDto MapTimeLinePost(IRecord arg)
    {
        var id = Guid.Parse(arg["id"].As<string>());
        var userRecord = arg["postedBy"].As<IReadOnlyDictionary<string, object>>();
        var user = new User(userRecord["username"].As<string>(), userRecord["displayName"].As<string>());
        var postedAt = arg["postedAt"].As<DateTime>();
        var body = arg["body"].As<string>();
        return new (id, user, postedAt, body);
    }
}