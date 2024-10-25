namespace Connextion.ViewModels.Profiles;

public class MessagesConversationViewModel(MessageService messageService, IMessageRepository messageRepository, User currentUser, string openProfileId)
{
    public bool IsBusy { get; private set; }
    public string NewMessage { get; set; } = "";
    public string? Error { get; private set; }
    public IAsyncEnumerable<MessageDto> Messages { get; } = messageRepository.GetMessagesAsync(currentUser.Id.Value, openProfileId);

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