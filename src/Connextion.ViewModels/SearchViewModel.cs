namespace Connextion.ViewModels;

public class SearchViewModel(PostService postService, IPostRepository postRepository)
{
    public bool IsBusy { get; private set; } = true;
    public IReadOnlyList<PostViewModel> PostResults { get; private set; } = Array.Empty<PostViewModel>();
    
    public async Task InitializeAsync(User currentUser, string query)
    {
        PostResults = await postRepository
            .SearchPostsAsync(query)
            .Select(post => new PostViewModel(postService, post, currentUser))
            .ToArrayAsync()
            .ConfigureAwait(false);
        IsBusy = false;
    }
}