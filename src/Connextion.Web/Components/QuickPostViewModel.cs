using Connextion.Posts;
using Microsoft.AspNetCore.Components;

namespace Connextion.Web.Components;

public class QuickPostViewModel(ILogger<QuickPostViewModel> logger, IPostRepository postRepository)
{
    public string StatusText { get; set; } = "";
    private string CurrentUser { get; set; } = "";
    
    public async Task SubmitAsync()
    {
        var status = new SubmitStatus(CurrentUser, StatusText);
        await postRepository.SubmitStatusAsync(status).ConfigureAwait(false);
        StatusText = "";
    }

    public void Initialize(string currentUser)
    {
        CurrentUser = currentUser;
    }

    
}