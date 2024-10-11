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
}

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string id);
}