namespace Connextion.ViewModels.Profiles;

public class MessagesViewModel(MessageService messageService, IMessageRepository messageRepository)
{
    // ttt 

    string? _openProfileId;
    User? _currentUser;

    public IReadOnlyList<InboxProfileDto> Inbox { get; private set; } = Array.Empty<InboxProfileDto>();
    public MessagesConversationViewModel? Conversation { get; private set; }

    public async Task InitializeAsync(User currentUser, string? openProfileId)
    {
        _currentUser = currentUser;
        _openProfileId = openProfileId;
        Conversation = openProfileId != null ? new(messageService, currentUser, openProfileId) : null;

        Inbox = await messageRepository.GetInboxAsync(currentUser.UserName).ConfigureAwait(false);
    }
}