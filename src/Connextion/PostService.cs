namespace Connextion;

public record LikePostCommand(PostId PostId, ProfileId ProfileId);
public record UnLikePostCommand(PostId PostId, ProfileId ProfileId);

public interface IPostRepository
{
    Task<Result> CreatePostAsync(PostCreated cmd);
    IAsyncEnumerable<Post> GetPostsByUserAsync(string profileId);
    IAsyncEnumerable<Post> GetTimeLineAsync(ProfileId currentUserId);
    Task<Result<Post>> LikeAsync(LikePostCommand cmd);
    Task<Result<Post>> UnLikeAsync(UnLikePostCommand cmd);
}

public class PostService(IPostRepository postRepository)
{
    public Task<Result> PostAsync(User currentUser, string body)
    {
        var created = currentUser.CreatePost(body);
        return created.BindAsync(postRepository.CreatePostAsync);
    }

    public Task<Result<Post>> LikeAsync(Post post, User currentUser)
    {
        return post.Like(currentUser).BindAsync(postRepository.LikeAsync);
    }

    public Task<Result<Post>> UnLikeAsync(Post post, User currentUser)
    {
        return post.UnLike(currentUser).BindAsync(postRepository.UnLikeAsync);
    }
}