using Connextion.ViewModels;

namespace Connextion.Web.Components.Posts;

public delegate QuickPostViewModel QuickPostViewModelFactory(UserDetails currentUser);

public class QuickPostViewModel(IPostRepository postRepository, UserDetails currentUser)
{
    public string StatusText { get; set; } = "";
    
    public async Task SubmitAsync()
    {
        var status = new CreatePostCmd(currentUser.username, StatusText);
        await postRepository.SubmitStatusAsync(status).ConfigureAwait(false);
        StatusText = "";
    }
}