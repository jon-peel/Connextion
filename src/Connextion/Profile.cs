namespace Connextion;

public class Profile(User user, IEnumerable<Post> latestPosts, IEnumerable<User> following, IEnumerable<User> followers)
{
    public User User { get; } = user;
    public IReadOnlyList<Post> LatestPosts { get; } = latestPosts.ToArray();
    public IReadOnlyList<User> Following { get; } = following.ToArray();
    public IReadOnlyList<User> Followers { get; } = followers.ToArray();

    public bool CanFollow(string toFollow)
    {
        return User.Username != toFollow && Following.All(x => x.Username != toFollow);
    }
}

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string username);
    Task FollowAsync(string currentUser, string toFollow);
}

public class ProfileService(IProfileRepository profileRepository)
{
    public async Task<bool> FollowAsync(string currentUser, string toFollow)
    {
        var profile = await profileRepository.GetProfileAsync(currentUser).ConfigureAwait(false);
        var result = profile.CanFollow(toFollow);
        if (result) await profileRepository.FollowAsync(currentUser, toFollow).ConfigureAwait(false);
        return result;
    }
}