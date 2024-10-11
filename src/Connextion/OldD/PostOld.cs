namespace Connextion.OldD;

public class PostOld(Guid id, MiniProfile postedBy, DateTime postedAt, string status)
{
    public Guid Id { get; } = id;
    public DateTime PostedAt { get; } = postedAt;
    public MiniProfile PostedBy { get; } = postedBy;
    public string Status { get; } = status;
}

public record CreatePostCmd(string Username, string Status, DateTime PostedAt);

public interface IPostRepositoryOld
{
    Task SubmitStatusAsync(CreatePostCmd status);
    Task<IReadOnlyList<PostOld>> GetTimelineStatusesAsync(User user);
}