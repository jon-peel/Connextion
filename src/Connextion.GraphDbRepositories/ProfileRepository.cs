using Connextion.OldD;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

class ProfileMapping(ProfileRepository profileRepository)
{
    public Profile Map(IRecord record)
    {
        var id = new ProfileId(record["id"].As<string>());
        var displayName = new DisplayName(record["displayName"].As<string>());
        var posts = profileRepository.GetPostsAsync(id.Value);
        var following = profileRepository.GetFollowingAsync(id.Value);
        var followers = profileRepository.GetFollowersAsync(id.Value);
        return new Profile(id, displayName, posts, following, followers);
    }
} 

public class ProfileRepository(IDriver driver) : IProfileRepository
{
    public async Task<Profile> GetProfileAsync(string id)
    {
        var (profiles, _) = await driver
            .ExecutableQuery("""
                             MATCH (profile:Profile { id: $id })
                             RETURN profile.id AS id,
                                    profile.displayName as displayName
                             """)
            .WithParameters(new { id })
            .WithMap(new ProfileMapping(this).Map)
            .ExecuteAsync()
            .ConfigureAwait(false);
        var profile = profiles.Single();
        return profile;
    }

    public async IAsyncEnumerable<PostOld> GetPostsAsync(string id)
    {
        const string query =
            """
            MATCH (profile:Profile { id: $id })
            MATCH (profile)<-[:POSTED_BY]-(post:Post)
            ORDER BY post.postedAt DESC 
            RETURN post.id AS id,
                   post.postedAt AS postedAt,
                   post.status AS status
            """;
        var session = driver.AsyncSession();
        var reader = await session.RunAsync(query, new { id }).ConfigureAwait(false);
        while (await reader.FetchAsync())
        {
            var post = Mapping.Post(reader.Current);
            yield return post;
        }
    }
    
    public async IAsyncEnumerable<MiniProfile> GetFollowingAsync(string id)
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
    
    public async IAsyncEnumerable<MiniProfile> GetFollowersAsync(string id)
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
}