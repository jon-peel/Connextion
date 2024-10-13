using Connextion.OldD;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class ProfileRepository(IDriver driver) : IProfileRepository
{
    public async Task<Profile> GetProfileAsync(string id)
    {
        const string query =
            """
            MATCH (profile:Profile { id: $id })
            RETURN profile.id AS id,
                   profile.displayName as displayName
            """;
        var (profiles, _) = await driver
            .ExecutableQuery(query)
            .WithParameters(new { id })
            .WithMap(MapProfile)
            .ExecuteAsync()
            .ConfigureAwait(false);
        var profile = profiles.Single();
        return profile;
    }

    public async  Task<Result> FollowAsync(Followed cmd)
    {
        const string query =
            """
            MATCH (currentUser:User {username: $currentUser})
            MATCH (toFollow:User {username: $toFollow})
            CREATE (currentUser)-[:FOLLOWS]->(toFollow)
            """;
        try
        {
            _ = await driver
                .ExecutableQuery(query)
                .WithParameters(new { currentUser = cmd.CurrentUser, toFollow = cmd.IsFollowing })
                .ExecuteAsync()
                .ConfigureAwait(false);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Error(e.Message);
        }
    }

    async IAsyncEnumerable<Post> GetPostsAsync(string id)
    {
        const string query =
            """
            MATCH (profile:Profile { id: $id })
            MATCH (profile)<-[:POSTED_BY]-(post:Post)
            ORDER BY post.postedAt DESC 
            RETURN post.id AS id,
                   post.postedAt AS postedAt,
                   post.body AS body
            """;
        var session = driver.AsyncSession();
        var reader = await session.RunAsync(query, new { id }).ConfigureAwait(false);
        while (await reader.FetchAsync())
        {
            var post = MapPost(reader.Current);
            yield return post;
        }
    }

    async IAsyncEnumerable<ProfileSummary> GetFollowingAsync(string id)
    {
        const string query =
            """
            MATCH (p:Profile { id: $id })-[:FOLLOWS]->(Profile)
            RETURN p.id as id,
                   p.displayName as displayName
            """;
        var session = driver.AsyncSession();
        var reader = await session.RunAsync(query, new { id }).ConfigureAwait(false);
        while (await reader.FetchAsync())
        {
            var p = Mapping.MiniProfile(reader.Current);
            yield return p;
        }
    }

    async IAsyncEnumerable<ProfileSummary> GetFollowersAsync(string id)
    {
        const string query =
            """
            MATCH (p:Profile { id: $id })<-[:FOLLOWS]-(Profile)
            RETURN p.id as id,
                   p.displayName as displayName
            """;
        var session = driver.AsyncSession();
        var reader = await session.RunAsync(query, new { id }).ConfigureAwait(false);
        while (await reader.FetchAsync())
        {
            var p = Mapping.MiniProfile(reader.Current);
            yield return p;
        }
    }

    Profile MapProfile(IRecord record)
    {
        var id = new ProfileId(record["id"].As<string>());
        var displayName = new DisplayName(record["displayName"].As<string>());
        var posts = GetPostsAsync(id.Value);
        var following = GetFollowingAsync(id.Value);
        var followers = GetFollowersAsync(id.Value);
        return new Profile(id, displayName, posts, following, followers);
    }

    Post MapPost(IRecord record)
    {
        var id = new PostId(record["id"].As<string>());
        var postedAt = record["postedAt"].As<DateTime>();
        var body = new PostBody(record["body"].As<string>());
        return new(id, postedAt, body);
    }
}