namespace Connextion;

public record Relationship(string Description, User User);

public class Profile(string fullName, IEnumerable<Post> latestPosts, IEnumerable<Relationship> relationships)
{
    public string FullName { get; } = fullName;
    public IReadOnlyList<Post> LatestPosts { get; } = latestPosts.ToArray();
    public IReadOnlyList<Relationship> RelationShips { get; } = relationships.ToArray();
}

public interface IProfileRepository
{
    Task<Profile> GetProfileAsync(string username);
}