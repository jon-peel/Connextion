using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class PostRepository(IDriver driver) : IPostRepository
{
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
            await driver
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
}