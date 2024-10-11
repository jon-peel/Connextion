using Connextion.OldD;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class ProfileRepositoryOld(ILogger<ProfileRepositoryOld> logger, IDriver driver) : IProfileRepositoryOld
{
    public async Task<OldD.Profile> GetProfileAsync(string user, User currentUser)
    {
        var (results, _) = await driver
            .ExecutableQuery("""
                             MATCH (u:User { username: $username })
                             MATCH (me:User { username: $currentUser })
                             RETURN 
                               { username: u.username, displayName: u.displayName } as user,
                               COLLECT {
                                   MATCH (p:Post)-[:POSTED_BY]->(u)
                                   RETURN {
                                      id: p.id,
                                      postedBy: { username: u.username, displayName: u.displayName },
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
                                   displayName: f.displayName,
                                   degrees: CASE WHEN me = f THEN 0
                                            ELSE length(shortestPath( (f)-[*]-(me) ))
                                            END
                                 }
                               } as following,
                               COLLECT {
                                 MATCH (u)<-[:FOLLOWS]-(f:User)
                                 RETURN { 
                                   username: f.username, 
                                   displayName: f.displayName,
                                   degrees: CASE WHEN me = f THEN 0
                                            ELSE length(shortestPath( (f)-[*]-(me) ))
                                            END
                                 }
                               } as followers
                             """)
            .WithParameters(new { user, currentUser = currentUser.Username, nPosts = 10 })
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