namespace Connextion.ViewModels;

public class CreatePostViewModel(PostService postService)
{
    User? _currentUser;
    public string Body { get; set; } = "";
    public bool CanPost => _currentUser is not null && !IsBusy;
    public bool IsBusy { get; private set; } = true;
    public ResultMessageViewModel? ResultMessage { get; private set; } 
    
    public async Task SubmitAsync()
    {
       if (!CanPost || _currentUser is null) return;
       IsBusy = true;
       var result = await postService.PostAsync(_currentUser, Body).ConfigureAwait(false);
       
       ResultMessage = new (result.Map(() => "Post created"));
       Body = "";
       IsBusy = false;
    }

    public void Initialize(User currentUser)
    {
        _currentUser = currentUser;
        IsBusy = false;
    }
}