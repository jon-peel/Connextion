using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class ProfileRepository(ILogger<ProfileRepository> logger, IDriver driver) : IProfileRepository
{
    public async Task<Profile> GetProfileAsync(string username, string currentUser)
    {
        var (results, _) = await driver
            .ExecutableQuery("""
                             MATCH (u:User { username: $username })
                             MATCH (me:User { username: $currentUser })
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
                                 RETURN { 
                                   username: f.username, 
                                   fullName: f.fullName,
                                   degrees: CASE WHEN me = f THEN 0
                                            ELSE length(shortestPath( (f)-[*]-(me) ))
                                            END
                                 }
                               } as following,
                               COLLECT {
                                 MATCH (u)<-[:FOLLOWS]-(f:User)
                                 RETURN { 
                                   username: f.username, 
                                   fullName: f.fullName,
                                   degrees: CASE WHEN me = f THEN 0
                                            ELSE length(shortestPath( (f)-[*]-(me) ))
                                            END
                                 }
                               } as followers
                             """)
            .WithParameters(new { username, currentUser, nPosts = 10 })
            .WithMap(Mapping.Profile)
            .ExecuteAsync()
            .ConfigureAwait(false);
        return results.Single();
    }

    public async Task FollowAsync(string currentUser, string toFollow)
    {
        logger.LogInformation("{FollowingUser} is following {FollowedUser}", currentUser, toFollow);
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