namespace Connextion;

public record ProfileId(string Value);

public record DisplayName(string Value);

public record PostId(Guid Value)
{
    public PostId(string value) : this(Guid.Parse(value))
    {
    }
}

public class ProfileSummary
{
    readonly Func<ProfileId, Task<byte>> _getDegreesFrom;

    public ProfileSummary(ProfileId id, DisplayName displayName, Func<ProfileId, Task<byte>> getDegreesFrom)
    {
        _getDegreesFrom = getDegreesFrom;
        Id = id;
        DisplayName = displayName;
    }
    
    public ProfileId Id { get; }
    public DisplayName DisplayName { get; }
    public Task<byte> GetDegreesFromAsync(ProfileId id) => _getDegreesFrom(id);
};

public record PostBody(string Value);

public record Post(PostId Id, DateTime PostedAt, PostBody Body);

public record PostCreated(PostId Id, ProfileId CreatedBy, DateTime PostedAt, PostBody Body);

public record FollowCmd(string CurrentUser, string IsFollowing);

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string id);
    Task<Result> FollowAsync(FollowCmd cmd);
}

public class Profile
{
    public Profile(ProfileId id,
        DisplayName displayName,
        IAsyncEnumerable<Post> posts,
        IAsyncEnumerable<ProfileSummary> following,
        IAsyncEnumerable<ProfileSummary> followers)
    {
        Id = id;
        DisplayName = displayName;
        Posts = posts;
        Following = following;
        Followers = followers;
    }

    public ProfileId Id { get; }
    public DisplayName DisplayName { get; }
    public IAsyncEnumerable<Post> Posts { get; }
    protected IAsyncEnumerable<ProfileSummary> Following { get; }
    protected IAsyncEnumerable<ProfileSummary> Followers { get; }
}