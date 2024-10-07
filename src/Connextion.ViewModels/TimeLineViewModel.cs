namespace Connextion.ViewModels;

public class TimeLineViewModel(IPostRepository postRepository)
{
    public IReadOnlyList<PostViewModel> Posts { get; private set; } = [];

    public async Task InitializeAsync(UserDetails user)
    {
        Posts = await GetPostsAsync(user).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<PostViewModel>> GetPostsAsync(UserDetails user)
    {
        var posts = await postRepository.GetTimelineStatusesAsync(user.username).ConfigureAwait(false);
        return posts.Select(post => new PostViewModel(post)).ToArray();
    }
}