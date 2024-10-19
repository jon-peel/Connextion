namespace Connextion.ViewModels;

public class TimeLineViewModel(PostService postService, IPostRepository postRepository)
{
    IAsyncEnumerable<PostViewModel>? _allPosts;

    public IReadOnlyList<PostViewModel> Posts { get; private set; } = [];

    public async Task InitializeAsync(User currentUser)
    {
        await InitializePostsAsync(currentUser).ConfigureAwait(false);
    }

    async Task InitializePostsAsync(User currentUser)
    {
        _allPosts = 
            postRepository
                .GetTimeLineAsync(currentUser.Id)
                .Select(post => new PostViewModel(postService, post, currentUser));
        Posts = await _allPosts.Take(50).ToArrayAsync().ConfigureAwait(false);
    }
}