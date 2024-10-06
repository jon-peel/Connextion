using Microsoft.Extensions.DependencyInjection;

namespace Connextion.ViewModels;

public record UserDetails(string UserName, string FullName);

public class TimeLineViewModel(IPostRepository postRepository)
{
    public IReadOnlyList<PostViewModel> Posts { get; private set; } = [];

    public async Task InitializeAsync(UserDetails user)
    {
        Posts = await GetPostsAsync(user).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<PostViewModel>> GetPostsAsync(UserDetails user)
    {
        var posts = await postRepository.GetTimelineStatusesAsync(user.UserName).ConfigureAwait(false);
        return posts.Select(PostViewModel.Create).ToArray();
    }
}

public class PostViewModel(DateTime postedAt, string postedBy, string status)
{
    public string Status => status;
    public string PostedBy => postedBy;

    public string Time => postedAt switch
    {
        { } t when t < DateTime.Today => t.ToString("HH:mm"),
        { } y when y < DateTime.Today.AddDays(-1) => $"Yesterday, {y:HH:mm}",
        { } w when w < DateTime.Today.AddDays(-7) => $"{Enum.GetName(w.DayOfWeek)}, {w:HH:mm}",
        _ => postedAt.ToString("d MMMM yyyy, HH:mm")
    };

    public string AuthorProfileUrl => $"/profile/{postedBy}";

    public static PostViewModel Create(Post post)
    {
        return new PostViewModel(post.PostedAt, post.PostedBy.FullName, post.Status);
    }

}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        return services
            .AddTransient<TimeLineViewModel>()
            .AddTransient<UserProfileViewModel>();
    }
}