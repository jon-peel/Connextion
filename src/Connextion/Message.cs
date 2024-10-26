namespace Connextion;

public record MessageId(Guid Value)
{
    public static MessageId Create() => new(Guid.NewGuid());
}

public record MessageBody(string Value);

public record SendMessageCmd(MessageId Id, DateTimeOffset SentAt, ProfileId From, ProfileId To, MessageBody Body);

public record InboxProfileDto(ProfileId Id, DisplayName DisplayName);

public record MessageDto(MessageId Id, ProfileId From, ProfileId To, DateTimeOffset SentAt, MessageBody Body);

public interface IMessageRepository
{
    Task<Result> SendMessageAsync(SendMessageCmd cmd);
    Task<IReadOnlyList<InboxProfileDto>> GetInboxAsync(ProfileId id);
    IAsyncEnumerable<MessageDto> GetMessagesAsync(string idValue, string openProfileId);
}

public class MessageService(IMessageRepository messageRepository)
{
    public static event Action<MessageDto>? MessageSent;

    public Task<Result> SendMessageAsync(string newMessage, User currentUser, string to)
    {
        var message = Message.Create(currentUser.Id, new(to), newMessage);
        return message
            .Send()
            .BindAsync(messageRepository.SendMessageAsync)
            .DoAsync(() => MessageSent?.Invoke(message.AsDto()));
    }
}

public class Message(MessageId id, DateTimeOffset sentAt, ProfileId from, ProfileId to, MessageBody body)
{
    public static Message Create(ProfileId from, ProfileId to, string body) =>
        new(MessageId.Create(), DateTimeOffset.Now, from, to, new(body));

    public Result<SendMessageCmd> Send()
    {
        if (string.IsNullOrEmpty(body.Value)) return Result<SendMessageCmd>.Error("body cannot be empty");
        if (body.Value.Length > 1000) return Result<SendMessageCmd>.Error("body cannot be longer than 1000 characters");

        return new SendMessageCmd(id, sentAt, from, to, body).ToResult();
    }

    public MessageDto AsDto() => new(id, from, to, sentAt, body);
}