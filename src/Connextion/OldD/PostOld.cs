namespace Connextion.OldD;

public class PostOld(Guid id, MiniProfile postedBy, DateTime postedAt, string body)
{
    public Guid Id { get; } = id;
    public DateTime PostedAt { get; } = postedAt;
    public MiniProfile PostedBy { get; } = postedBy;
    public string Body { get; } = body;
}

public record CreatePostCmd(string Username, string Body, DateTime PostedAt);

