using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class ProfileRepository(IDriver driver) : IProfileRepository
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
                                   RETURN {
                                      id: p.id,
                                      postedBy: { userName: u.userName, fullName: u.fullName },
                                      postedAt: p.postedAt, 
                                      status: p.status 
                                   }
                                   ORDER BY p.postedAt DESC
                                   LIMIT $nPosts
                               } AS posts;
                             """)
            .WithParameters(new { userName, nPosts = 10 })
            .WithMap(Mapping.Profile)
            .ExecuteAsync()
            .ConfigureAwait(false);
        return results.Single();
    }
}