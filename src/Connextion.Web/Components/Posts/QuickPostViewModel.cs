using Connextion.Posts;
using Connextion.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Connextion.Web.Components.Posts;

public delegate QuickPostViewModel QuickPostViewModelFactory(UserDetails currentUser);

public class QuickPostViewModel(IPostRepository postRepository, UserDetails currentUser)
{
    public string StatusText { get; set; } = "";
    
    public async Task SubmitAsync()
    {
        var status = new SubmitStatus(currentUser.UserName, StatusText);
        await postRepository.SubmitStatusAsync(status).ConfigureAwait(false);
        StatusText = "";
    }
}