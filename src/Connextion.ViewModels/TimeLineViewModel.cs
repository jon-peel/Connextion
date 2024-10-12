namespace Connextion.ViewModels;

public class TimeLineViewModel(IPostRepository postRepository)
{
    IAsyncEnumerable<PostViewModel>? _allPosts;

    public IReadOnlyList<PostViewModel> Posts { get; private set; } = [];

    public async Task InitializeAsync(Profile currentUser)
    {
        await InitializePostsAsync(currentUser).ConfigureAwait(false);
    }

    async Task InitializePostsAsync(Profile currentUser)
    {
        _allPosts = 
            postRepository
                .GetTimeLineAsync(currentUser.Id)
                .Select(post => new PostViewModel(post));
        Posts = await _allPosts.Take(50).ToArrayAsync().ConfigureAwait(false);
    }
}