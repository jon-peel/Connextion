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