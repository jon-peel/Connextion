namespace Connextion.ViewModels;

public class LikeButtonViewModel(PostService postService, Post post, User currentUser)
{
    public bool CanBeLiked { get; private set; } = !post.LikedBy.Contains(currentUser.Id);
    public bool CanBeUnLiked { get; private set; } = !post.LikedBy.Contains(currentUser.Id);
    public string ButtonText { get; private set; } = GenerateButtonText(post, currentUser);


    public async Task LikeAsync()
    {
        (CanBeLiked, CanBeUnLiked, ButtonText) =
            await postService.LikeAsync(post, currentUser)
                .MapAsync(p => (false, true, GenerateButtonText(p, currentUser)))
                .DefaultAsync(_ => (true, false, ButtonText))
                .ConfigureAwait(false);
    }


    public async Task UnLikeAsync()
    {
        (CanBeLiked, CanBeUnLiked, ButtonText) =
            await postService.UnLikeAsync(post, currentUser)
                .MapAsync(p => (false, true, GenerateButtonText(p, currentUser)))
                .DefaultAsync(_ => (true, false, ButtonText))
                .ConfigureAwait(false);
    }

    static string GenerateButtonText(Post post, User user)
    {
        var liked = $"Liked by {post.LikedBy.Count()}";
        var including = post.LikedBy.Contains(user.Id) ? ", including you" : ".";
        return liked + including;
    }
}