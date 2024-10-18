namespace Connextion;

public record ProfileId(string Value);
public record DisplayName(string Value);
public record Bio(string Value);

public record PostId(Guid Value)
{
    public PostId(string value) : this(Guid.Parse(value))
    {
    }
}

public class ProfileSummary
{
    readonly Func<ProfileId, Task<byte>> _getDegreesFrom;

    public ProfileSummary(ProfileId id, DisplayName displayName, Bio bio, Func<ProfileId, Task<byte>> getDegreesFrom)
    {
        _getDegreesFrom = getDegreesFrom;
        Id = id;
        DisplayName = displayName;
        Bio = bio;
    }
    
    public ProfileId Id { get; }
    public DisplayName DisplayName { get; }
    public Bio Bio { get; }
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
    Task<Result> UpdateBioAsync(UpdateBioCmd arg);
}

public class Profile : ProfileSummary
{
    public Profile(ProfileId id,
        DisplayName displayName,
        Bio bio,
        IAsyncEnumerable<Post> posts,
        IAsyncEnumerable<ProfileSummary> following,
        IAsyncEnumerable<ProfileSummary> followers)
    : base(id, displayName, bio,_ => Task.FromResult<byte>(0))
    {
        Posts = posts;
        Following = following;
        Followers = followers;
    } 
    public IAsyncEnumerable<Post> Posts { get; }
    public IAsyncEnumerable<ProfileSummary> Following { get; }
    public IAsyncEnumerable<ProfileSummary> Followers { get; }
}