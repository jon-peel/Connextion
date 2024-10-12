using Connextion.OldD;

namespace Connextion;

public interface IPostRepository
{
    Task<Result> CreatePostAsync(PostCreated cmd);
}

public class PostService(IPostRepository postRepository)
{
    public Task<Result> PostAsync(Profile currentUser, string body)
    {
        var created = currentUser.CreatePost(body);
        return created.BindAsync(postRepository.CreatePostAsync);
    }
}