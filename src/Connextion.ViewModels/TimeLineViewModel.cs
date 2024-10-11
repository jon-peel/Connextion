namespace Connextion.ViewModels;

public class TimeLineViewModel(IPostRepository postRepository)
{
    public IReadOnlyList<PostViewModel> Posts { get; private set; } = [];

    public async Task InitializeAsync(CurrentUser user)
    {
        Posts = await GetPostsAsync(user).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<PostViewModel>> GetPostsAsync(CurrentUser user)
    {
        // if (user is null) return Array.Empty<PostViewModel>();
        var postResults = await postRepository.GetTimelineStatusesAsync(user).ConfigureAwait(false);
        var posts = postResults.Select(post => new PostViewModel(post)).ToArray();
        return posts;
    }
}