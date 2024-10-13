using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Connextion.ViewModels;

public class ResultMessageViewModel : INotifyPropertyChanged
{
    bool _isVisible;

    public ResultMessageViewModel(Result<string> result)
    {
        (Message, IsSuccess)
            = result
                .Map(msg => (msg, true))
                .Default(err => (err, false));
        IsVisible = true;
        _ = HideAfterTenAsync();
    }

    public bool IsVisible
    {
        get => _isVisible;
        private set => SetField(ref _isVisible, value);
    }

    public string Message { get; }
    public bool IsSuccess { get; }

    async Task HideAfterTenAsync()
    {
        await Task.Delay(10_000).ConfigureAwait(false);
        IsVisible = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new (propertyName));
    }

    void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}

public class CreatePostViewModel(PostService postService)
{
    Profile? _currentUser;
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

    public void Initialize(Profile currentUser)
    {
        _currentUser = currentUser;
        IsBusy = false;
    }
}