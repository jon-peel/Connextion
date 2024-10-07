namespace Connextion;

public class Post(Guid id, User postedBy, DateTime postedAt, string status)
{
    public Guid Id { get; } = id;
    public DateTime PostedAt { get; } = postedAt;
    public User PostedBy { get; } = postedBy;
    public string Status { get; } = status;
}

public record CreatePostCmd(string Username, string Status);

public interface IPostRepository
{
    Task SubmitStatusAsync(CreatePostCmd status);
    Task<IReadOnlyList<Post>> GetTimelineStatusesAsync(string username);
}