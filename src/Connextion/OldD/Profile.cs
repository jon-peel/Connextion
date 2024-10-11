namespace Connextion.OldD;

public class Profile(
    User user, 
    IEnumerable<PostOld> latestPosts, 
    IEnumerable<MiniProfile> following, 
    IEnumerable<MiniProfile> followers)
{
    public User User { get; } = user;
    public IReadOnlyList<PostOld> LatestPosts { get; } = latestPosts.ToArray();
    public IReadOnlyList<MiniProfile> Following { get; } = following.ToArray();
    public IReadOnlyList<MiniProfile> Followers { get; } = followers.ToArray();

    public bool CanFollow(string toFollow)
    {
        return User.Username != toFollow && Following.All(x => x.Username != toFollow);
    }
}

public interface IProfileRepositoryOld
{
    Task<Profile> GetProfileAsync(string user, User currentUuser);
    Task FollowAsync(string currentUser, string toFollow);
}

public class ProfileService(IProfileRepositoryOld profileRepositoryOld)
{
    public async Task<bool> FollowAsync(User user, string toFollow)
    {
        var profile = await profileRepositoryOld.GetProfileAsync(user.Username, user).ConfigureAwait(false);
        var result = profile.CanFollow(toFollow);
        if (result) await profileRepositoryOld.FollowAsync(user.Username, toFollow).ConfigureAwait(false);
        return result;
    }
}