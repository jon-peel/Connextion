namespace Connextion.ViewModels.Profiles;

public class MessagesConversationViewModel(MessageService messageService, User currentUser, string openProfileId)
{
    // IAsyncEnumerable<MessageDto> _messages = messageRepository.GetMessagesAsync(currentUser.UserName, openProfileId);
    public bool IsBusy { get; private set; }
    public string NewMessage { get; set; } = "";
    public string? Error { get; private set; }

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