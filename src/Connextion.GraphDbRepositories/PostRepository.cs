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

    public IAsyncEnumerable<Post> GetPostsByUserAsync(string profileId) =>
        userRepository.GetPostsByUserAsync(profileId);

    public IAsyncEnumerable<Post> GetTimeLineAsync(ProfileId id)
    {
        const string query =
            """
            MATCH (post:Post)-[:POSTED_BY]->(postedBy:Profile)<-[:FOLLOWS]-(currentUser:User)
            WHERE currentUser.id = $id
            ORDER by post.postedAt DESC
            RETURN post.id AS id,
                   { id: postedBy.id, displayName: postedBy.displayName, bio: postedBy.bio } AS postedBy,
                   post.postedAt AS postedAt,
                   post.body AS body,
                   COLLECT { MATCH (post)<-[:LIKES]-(likedBy:Profile)
                             RETURN likedBy.id } AS likedBy
            """;
        return ExecuteReaderQueryAsync(query, new { id = id.Value }, userRepository.MapPost);
    }

    public async Task<Result<Post>> LikeAsync(LikePostCommand cmd)
    {
        var query =
            """
            MATCH (profile:Profile { id: $profileId })
            MATCH (post:Post { id: $postId })-[:POSTED_BY]->(postedBy:Profile)
            CREATE (profile)-[:LIKES]->(post)
            RETURN post.id AS id,
                   { id: postedBy.id, displayName: postedBy.displayName, bio: postedBy.bio } AS postedBy,
                   post.postedAt AS postedAt,
                   post.body AS body,
                   COLLECT { MATCH (post)<-[:LIKES]-(likedBy:Profile)
                             RETURN likedBy.id } AS likedBy
            """;
        var parameters = new { profileId = cmd.ProfileId.Value, postId = cmd.PostId.Value.ToString() };
        var result = await ExecuteQueryAsync(query, parameters, userRepository.MapPost).ConfigureAwait(false);
        return result.Single().ToResult();
    }

    public Task<Result<Post>> UnLikeAsync(UnLikePostCommand cmd)
    {
        throw new NotImplementedException();
    }
}