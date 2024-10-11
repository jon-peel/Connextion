using Connextion.OldD;

namespace Connextion;

public record ProfileId(string Value);

public record DisplayName(string Value);

public class Profile(
    ProfileId id,
    DisplayName displayName,
    IAsyncEnumerable<PostOld> posts, 
    IAsyncEnumerable<MiniProfile> following, 
    IAsyncEnumerable<MiniProfile> followers)
{
    public ProfileId Id { get; } = id;
    public DisplayName DisplayName { get; } = displayName;
    public IAsyncEnumerable<PostOld> Posts { get; } = posts;
    public IAsyncEnumerable<MiniProfile> Following { get; } = following;
    public IAsyncEnumerable<MiniProfile> Followers { get; } = followers;
}

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string id);
}