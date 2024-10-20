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
        // To return any post created by, or liked by someone the current user follows.
        const string query =
            """
            MATCH (fol:Profile)<-[:FOLLOWS]-(currentUser:User:Profile { id: $id })
            MATCH (fol)-[:LIKES]->(p1:Post)
            WITH collect(p1) as likedPost
            MATCH (fol)<-[:POSTED_BY]-(p2:Post)
            WITH collect(p2) as posted, likedPost
            WITH posted + likedPost as allPosts
            UNWIND allPosts AS post
            WITH DISTINCT post
            MATCH (post)-[:POSTED_BY]->(postedBy:Profile)
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

    public async Task<Result<Post>> UnLikeAsync(UnLikePostCommand cmd)
    {
        const string query =
            """
            MATCH (profile:Profile { id: $profileId })-[like:LIKES]->(post:Post { id: $postId })
            MATCH (post)-[:POSTED_BY]->(postedBy:Profile)
            DELETE like
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
}