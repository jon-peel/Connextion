namespace Connextion.ViewModels.Profiles;

public class MessagesConversationViewModel(MessageService messageService, IMessageRepository messageRepository, User currentUser, string openProfileId)
{
    public bool IsBusy { get; private set; }
    public string NewMessage { get; set; } = "";
    public string? Error { get; private set; }
    public IAsyncEnumerable<MessageBlockViewModel> Messages { get; } = 
        messageRepository
            .GetMessagesAsync(currentUser.Id.Value, openProfileId)
            .Select(dto => new MessageBlockViewModel(currentUser, dto));

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
}

public class MessageBlockViewModel(User currentUser, MessageDto message)
{
    public bool FromMe { get; } = currentUser.DisplayName.Value == message.From.Value;
    public string Message { get; } = message.Body.Value;
    public string SentAt { get; } = message.SentAt.ToString("HH:mm");
    public string From { get; } = message.From.Value;
}