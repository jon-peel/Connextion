namespace Connextion;

public class Post(DateTime postedAt, User postedBy, string status )
{
    public DateTime PostedAt { get; } = postedAt;
    public User PostedBy { get; } = postedBy;
    public string Status { get; } = status;
}

public record CreatePostCmd(string UserName, string Status);

public interface IPostRepository
{
    Task SubmitStatusAsync(CreatePostCmd status);
    Task<Post[]> GetTimelineStatusesAsync(string userName);
}