namespace Connextion;

public class Post(Guid id, MiniProfile postedBy, DateTime postedAt, string status)
{
    public Guid Id { get; } = id;
    public DateTime PostedAt { get; } = postedAt;
    public MiniProfile PostedBy { get; } = postedBy;
    public string Status { get; } = status;
}

public record CreatePostCmd(string Username, string Status, DateTime PostedAt);

public interface IPostRepository
{
    Task SubmitStatusAsync(CreatePostCmd status);
    Task<IReadOnlyList<Post>> GetTimelineStatusesAsync(CurrentUser user);
}