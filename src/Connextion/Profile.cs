namespace Connextion;

public class Profile(string fullName, IEnumerable<Post> latestPosts, IEnumerable<User> following, IEnumerable<User> followers)
{
    public string FullName { get; } = fullName;
    public IReadOnlyList<Post> LatestPosts { get; } = latestPosts.ToArray();
    public IReadOnlyList<User> Following { get; } = following.ToArray();
    public IReadOnlyList<User> Followers { get; } = followers.ToArray();
}

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string username);
}