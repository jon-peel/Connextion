using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

class ProfileRepository(IDriver driver, IUserRepository userRepository) : RepositoryBase(driver), IProfileRepository
{
    public async Task<Profile> GetProfileAsync(string id)
    {
        Profile profile = await userRepository.GetUserAsync(id);
        return profile;
    }

    public Task<Result> FollowAsync(FollowCmd cmd)
    {
        const string query =
            """
            MATCH (currentUser:User {username: $currentUser})
            MATCH (toFollow:User {username: $toFollow})
            CREATE (currentUser)-[:FOLLOWS]->(toFollow)
            """;
        var parameters = new { currentUser = cmd.CurrentUser, toFollow = cmd.IsFollowing };
        return ExecuteWriteAsync(query, parameters);
    }

    public Task<Result> UpdateBioAsync(UpdateBioCmd arg)
    {
        const string query =
            """
            MATCH (profile:Profile {profileId: $id})
            SET profile.bio = $bio
            """;
        var parameters = new { id = arg.ProfileId, bio = arg.Bio };
        return ExecuteWriteAsync(query, parameters);
    }
}