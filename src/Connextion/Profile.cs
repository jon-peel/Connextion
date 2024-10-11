namespace Connextion;

public class Profile(
    User user, 
    IEnumerable<Post> latestPosts, 
    IEnumerable<MiniProfile> following, 
    IEnumerable<MiniProfile> followers)
{
    public User User { get; } = user;
    public IReadOnlyList<Post> LatestPosts { get; } = latestPosts.ToArray();
    public IReadOnlyList<MiniProfile> Following { get; } = following.ToArray();
    public IReadOnlyList<MiniProfile> Followers { get; } = followers.ToArray();

    public bool CanFollow(string toFollow)
    {
        return User.Username != toFollow && Following.All(x => x.Username != toFollow);
    }
}

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string user, CurrentUser currentUser);
    Task FollowAsync(string currentUser, string toFollow);
}

public class ProfileService(IProfileRepository profileRepository)
{
    public async Task<bool> FollowAsync(CurrentUser currentUser, string toFollow)
    {
        var profile = await profileRepository.GetProfileAsync(currentUser.Username, currentUser).ConfigureAwait(false);
        var result = profile.CanFollow(toFollow);
        if (result) await profileRepository.FollowAsync(currentUser.Username, toFollow).ConfigureAwait(false);
        return result;
    }
}