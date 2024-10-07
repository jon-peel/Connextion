using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class ProfileRepository(IDriver driver) : IProfileRepository
{
    public async Task<Profile> GetProfileAsync(string username)
    {
        var (results, _) = await driver
            .ExecutableQuery("""
                             MATCH (u:User {username: $username} ) 
                             RETURN 
                               { username: u.username, fullName: u.fullName } as user,
                               COLLECT {
                                   MATCH (p:Post)-[:POSTED_BY]->(u)
                                   RETURN {
                                      id: p.id,
                                      postedBy: { username: u.username, fullName: u.fullName },
                                      postedAt: p.postedAt, 
                                      status: p.status 
                                   }
                                   ORDER BY p.postedAt DESC
                                   LIMIT $nPosts
                               } AS posts,
                               COLLECT {
                                 MATCH (u)-[:FOLLOWS]->(f:User)
                                 RETURN { username: f.username, fullName: f.fullName }
                               } as following,
                               COLLECT {
                                 MATCH (u)<-[:FOLLOWS]-(f:User)
                                 RETURN { username: f.username, fullName: f.fullName }
                               } as followers
                             """)
            .WithParameters(new { username, nPosts = 10 })
            .WithMap(Mapping.Profile)
            .ExecuteAsync()
            .ConfigureAwait(false);
        return results.Single();
    }

    public async Task FollowAsync(string currentUser, string toFollow)
    {
        var (_, results) = await driver
            .ExecutableQuery("""
                             MATCH (currentUser:User {username: $currentUser}),
                                   (toFollow:User {username: $toFollow})
                             CREATE (currentUser)-[:FOLLOWS]->(toFollow);
                             """)
            .WithParameters(new { currentUser, toFollow })
            .ExecuteAsync()
            .ConfigureAwait(false);
        Console.WriteLine(results);
    }  
}