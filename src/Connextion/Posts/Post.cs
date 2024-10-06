using Connextion.Graph;

namespace Connextion.Posts;

public class Post(DateTime postedAt, User postedBy, string status )
{
    public DateTime PostedAt { get; } = postedAt;
    public User PostedBy { get; } = postedBy;
    public string Status { get; } = status;
}

public record SubmitStatus(string PostedBy, string Status);

public record TimelineStatus(Guid id, string PostedBy, DateTime PostedAt, string Status);

public interface IPostRepository
{
    Task SubmitStatusAsync(SubmitStatus status);
    Task<TimelineStatus[]> GetTimelineStatusesAsync(string userName);
    Task<IReadOnlyList<TimelineStatus>> GetUserPostsAsync(string userName, int i);
}