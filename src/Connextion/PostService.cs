using Connextion.OldD;

namespace Connextion;

public record TimeLinePostDto(Guid Id, User PostedBy, DateTime PostedAt, string Body);

public interface IPostRepository
{
    Task<Result> CreatePostAsync(PostCreated cmd);
    IAsyncEnumerable<TimeLinePostDto> GetTimeLineAsync(ProfileId currentUserId);
}

public class PostService(IPostRepository postRepository)
{
    public Task<Result> PostAsync(Profile currentUser, string body)
    {
        var created = currentUser.CreatePost(body);
        return created.BindAsync(postRepository.CreatePostAsync);
    }
}