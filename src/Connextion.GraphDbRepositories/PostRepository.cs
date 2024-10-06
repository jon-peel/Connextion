using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

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

    public async Task<Post[]> GetTimelineStatusesAsync(string userName)
    {
        var (result, _) = await driver
            .ExecutableQuery(
                """
                MATCH (p:Post)-[:POSTED_BY]->(u:User)
                return p.id, p.status, p.postedAt, u.userName as postedBy, u.name as postedByName
                """)
            .ExecuteAsync()
            .ConfigureAwait(false);
        var timelineStatuses = result
            .Select(r => new Post(
                r["p.postedAt"].As<DateTime>(),
                //Guid.Parse(r["p.id"].As<string>()),
                new User(r["postedBy"].As<string>(), r["postedByName"].As<string>() ),
                r["p.status"].As<string>()
            ))
            .ToArray();
        return timelineStatuses;
    }
}