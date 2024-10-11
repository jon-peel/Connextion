using Connextion.OldD;

namespace Connextion.ViewModels;

public class TimeLineViewModel(IPostRepositoryOld old)
{
    public IReadOnlyList<PostViewModel> Posts { get; private set; } = [];

    public async Task InitializeAsync(CurrentUser user)
    {
        Posts = await GetPostsAsync(user).ConfigureAwait(false);
    }

    async Task<IReadOnlyList<PostViewModel>> GetPostsAsync(CurrentUser user)
    {
        // if (user is null) return Array.Empty<PostViewModel>();
        var postResults = await old.GetTimelineStatusesAsync(user).ConfigureAwait(false);
        var posts = postResults.Select(post => new PostViewModel(post)).ToArray();
        return posts;
    }
}