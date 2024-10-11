using Connextion.OldD;

namespace Connextion.ViewModels;

public class QuickPostViewModel(IPostRepositoryOld old)
{
    User? _currentUser;
    public string StatusText { get; set; } = "";
    
    public async Task SubmitAsync()
    {
        if (_currentUser is { Username: { } username })
        {
            var status = new CreatePostCmd(username, StatusText, DateTime.Now);
            await old.SubmitStatusAsync(status).ConfigureAwait(false);
            StatusText = "";
        }
    }

    public void Initialize(User user)
    {
        _currentUser = user;
    }
}