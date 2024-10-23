namespace Connextion;

public record MessageId(Guid Value) {
    public static MessageId Create() => new(Guid.NewGuid());
}

public record MessageBody(string Value);
public record SendMessageCmd(MessageId Id, DateTime SentAt, ProfileId From, ProfileId To, MessageBody Body);
public record InboxProfileDto();
public record MessageDto();

public interface IMessageRepository
{
    Task<Result> SendMessageAsync(SendMessageCmd arg);
}

public class MessageService(IMessageRepository messageRepository)
{
    public Task<Result> SendMessageAsync(string newMessage, User currentUser, string to)
    {
        var message = Message.Create(currentUser.Id, new(to), newMessage);
        return message.Send().BindAsync(messageRepository.SendMessageAsync);
    }
}

public class Message(MessageId id, DateTime sentAt, ProfileId from, ProfileId to, MessageBody body)
{
    public static Message Create(ProfileId from, ProfileId to, string body) => 
        new (MessageId.Create(), DateTime.UtcNow, from, to, new(body));

    public Result<SendMessageCmd> Send()
    {
        if (string.IsNullOrEmpty(body.Value)) return Result<SendMessageCmd>.Error("body cannot be empty");
        if (body.Value.Length > 1000) return Result<SendMessageCmd>.Error("body cannot be longer than 1000 characters");

        return new SendMessageCmd(id, sentAt, from, to, body).ToResult();
    }
}

