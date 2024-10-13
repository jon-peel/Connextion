namespace Connextion;

public record ProfileId(string Value);

public record DisplayName(string Value);

public record PostId(Guid Value)
{
    public PostId(string value) : this(Guid.Parse(value))
    {
    }
}

public record ProfileSummary(ProfileId Id);

public record PostBody(string Value);

public record Post(PostId Id, DateTime PostedAt, PostBody Body);

public record PostCreated(PostId Id, ProfileId CreatedBy, DateTime PostedAt, PostBody Body);

public record Followed(string CurrentUser, string IsFollowing);

public class Profile
{
    internal Profile(ProfileId id,
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

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string id);
    Task<Result> FollowAsync(Followed cmd);
}