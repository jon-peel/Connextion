using Neo4j.Driver;

namespace Connextion.Posts;

public record SubmitStatus(string PostedBy, string Status);

public record TimelineStatus(Guid id, string PostedBy, DateTime PostedAt, string Status);

public interface IPostRepository
{
    Task SubmitStatusAsync(SubmitStatus status);
    Task<TimelineStatus[]> GetTimelineStatusesAsync(string userName);
}

public class PostRepository(IDriver driver) : IPostRepository
{
    public async Task SubmitStatusAsync(SubmitStatus status)
    {
        var parameters = new
        {
            userName = status.PostedBy,
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

    public async Task<TimelineStatus[]> GetTimelineStatusesAsync(string userName)
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
            .Select(r => new TimelineStatus(
                Guid.Parse(r["p.id"].As<string>()),
                r["postedBy"].As<string>(),
                r["p.postedAt"].As<DateTime>(),
                r["p.status"].As<string>()
            ))
            .ToArray();
        return timelineStatuses;
    }
}