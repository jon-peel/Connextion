using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace Connextion.ViewModels.Profiles;

public class MessagesConversationViewModel(MessageService messageService, IMessageRepository messageRepository, User currentUser, string openProfileId)
    : INotifyPropertyChanged
{
    IList<MessageBlockViewModel> _messages = [];
    public bool IsBusy { get; private set; }
    public string NewMessage { get; set; } = "";
    public string? Error { get; private set; }
    public IEnumerable<MessageBlockViewModel> Messages => _messages;

    ~MessagesConversationViewModel()
    {
        MessageService.MessageSent -= OnMessageSent;
    }
    
    public async Task InitializeAsync()
    {
        _messages = await messageRepository
            .GetMessagesAsync(currentUser.Id.Value, openProfileId)
            .Select(dto => new MessageBlockViewModel(currentUser, dto))
            .ToListAsync();
        
        MessageService.MessageSent += OnMessageSent;
    }

    void OnMessageSent(MessageDto dto)
    {
        if ((dto.From.Value != openProfileId || dto.To.Value != currentUser.Id.Value)
            && (dto.To.Value != openProfileId || dto.From.Value != currentUser.Id.Value)) return;
        var vm = new MessageBlockViewModel(currentUser, dto);
        _messages.Add(vm);
        OnPropertyChanged(nameof(Messages));
    }


    public async Task SendMessageAsync()
    {
        IsBusy = true;
        (NewMessage, Error) = await messageService
            .SendMessageAsync(NewMessage, currentUser, openProfileId)
            .MapAsync(() => ("", default(string)))
            .DefaultAsync(e => (NewMessage, e))
            .ConfigureAwait(false);
        IsBusy = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class MessageBlockViewModel(User currentUser, MessageDto message)
{
    public bool FromMe { get; } = currentUser.DisplayName.Value == message.From.Value;
    public string Message { get; } = message.Body.Value;
    public string SentAt { get; } = message.SentAt.ToString("HH:mm");
    public string From { get; } = message.From.Value;
}