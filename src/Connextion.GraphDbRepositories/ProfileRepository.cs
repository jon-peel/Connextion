using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class ProfileRepository(IDriver driver, IPostRepository postRepository) : IProfileRepository
{
    public async Task<Profile> GetProfileAsync(string userName)
    {
        var (results, _) = await driver
            .ExecutableQuery("""
                             MATCH (u:User {userName: $userName} ) 
                             RETURN 
                               u.fullName AS fullName,
                               COLLECT {
                                   MATCH (p:Post)-[:POSTED_BY]->(u)
                                   RETURN { postedAt: p.postedAt, status: p.status }
                                   ORDER BY p.postedAt DESC
                                   LIMIT $nPosts
                               } AS posts;
                             """)
            .WithParameters(new { userName, nPosts = 10 })
            .WithMap(r => MapProfile(userName, r))
            .ExecuteAsync()
            .ConfigureAwait(false);
        return results.Single();
    }

    private Profile MapProfile(string userName, IRecord record)
    {
        var fullName = record["fullName"].As<string>();
        var latestPosts = record["posts"]
            .As<IEnumerable<Dictionary<string, object>>>()
            .Select(r => new Post( 
                r["postedAt"].As<DateTime>(),
                new User(userName, fullName),
                r["status"].As<string>()
            ))
            .ToArray();
        return new Profile(fullName, latestPosts, []);
    }
}