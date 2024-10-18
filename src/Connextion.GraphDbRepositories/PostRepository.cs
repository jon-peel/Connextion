using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

class PostRepository(IDriver driver, UserRepository userRepository) : RepositoryBase(driver), IPostRepository
{
    // readonly IDriver _driver = driver;

    public Task<Result> CreatePostAsync(PostCreated cmd)
    {
        const string query =
            """
            MATCH (profile:Profile { id: $profileId })
            CREATE (post:Post { id: $postId, body: $body, postedAt: $postedAt })
            CREATE (post)-[:POSTED_BY]->(profile)
            """;
        var parameters = new
        {
            profileId = cmd.CreatedBy.Value,
            postId = cmd.Id.Value.ToString(),
            body = cmd.Body.Value,
            postedAt = cmd.PostedAt.ToLocalTime()
        };
        return ExecuteWriteAsync(query, parameters);
    }

    public IAsyncEnumerable<TimeLinePostDto> GetTimeLineAsync(ProfileId id)
    {
        const string query =
            """
            MATCH (post:Post)-[:POSTED_BY]->(postedBy:Profile)<-[:FOLLOWS]-(currentUser:User)
            WHERE currentUser.id = $id
            ORDER by post.postedAt DESC
            RETURN post.id AS id,
                   { id: postedBy.id, displayName: postedBy.displayName, bio: postedBy.bio } AS postedBy,
                   post.postedAt AS postedAt,
                   post.body AS body
            """;
        return ExecuteReaderQueryAsync(query, new { id = id.Value }, MapTimeLinePost);
    }

    TimeLinePostDto MapTimeLinePost(IRecord arg)
    {
        var id = Guid.Parse(arg["id"].As<string>());
        var postedBy = userRepository.MapProfileSummary(arg["postedBy"].As<IReadOnlyDictionary<string, object>>());
        var postedAt = arg["postedAt"].As<DateTime>();
        var body = arg["body"].As<string>();
        return new(id, postedBy, postedAt, body);
    }
}