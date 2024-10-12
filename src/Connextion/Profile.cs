using Connextion.OldD;

namespace Connextion;

public record ProfileId(string Value);
public record DisplayName(string Value);

public record PostId(Guid Value)
{
    public PostId(string value) : this(Guid.Parse(value)) { }
}
public record PostBody(string Value);
public record Post(PostId Id, DateTime PostedAt, PostBody Body);

public record PostCreated(PostId Id, ProfileId CreatedBy, DateTime PostedAt, PostBody Body);


public class Profile(
    ProfileId id,
    DisplayName displayName,
    IAsyncEnumerable<Post> posts, 
    IAsyncEnumerable<MiniProfile> following, 
    IAsyncEnumerable<MiniProfile> followers)
{
    public ProfileId Id { get; } = id;
    public DisplayName DisplayName { get; } = displayName;
    public IAsyncEnumerable<Post> Posts { get; } = posts;
    public IAsyncEnumerable<MiniProfile> Following { get; } = following;
    public IAsyncEnumerable<MiniProfile> Followers { get; } = followers;

    public Result<PostCreated> CreatePost(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return Result<PostCreated>.Error("body cannot be empty");
        if (body.Length > 500) return Result<PostCreated>.Error("body cannot be longer than 500 characters");

        var id = new PostId(Guid.NewGuid());
        var created = new PostCreated(id, Id, DateTime.UtcNow, new (body));
        return created;
    }
}

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string id);
}