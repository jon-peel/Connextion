namespace Connextion.ViewModels;

public class QuickPostViewModel(IPostRepository postRepository)
{
    CurrentUser? _currentUser;
    public string StatusText { get; set; } = "";
    
    public async Task SubmitAsync()
    {
        if (_currentUser is { Username: { } username })
        {
            var status = new CreatePostCmd(username, StatusText, DateTime.Now);
            await postRepository.SubmitStatusAsync(status).ConfigureAwait(false);
            StatusText = "";
        }
    }

    public void Initialize(CurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
}