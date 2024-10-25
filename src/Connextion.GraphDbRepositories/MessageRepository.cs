using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class MessageRepository(IDriver driver) : RepositoryBase(driver), IMessageRepository
{
    public Task<Result> SendMessageAsync(SendMessageCmd cmd)
    {
        const string query =
            """
            MATCH (from:Profile {id: $from})
            MATCH (to:Profile {id: $to})
            CREATE (from)-[m:MESSAGE]->(to)
            SET m.id = $id
            SET m.sentAt = $sentAt
            SET m.body = $body
            """;
        var parameters = new
        {
            id = cmd.Id.Value.ToString(),
            from = cmd.From.Value,
            to = cmd.To.Value,
            sentAt = cmd.SentAt,
            body = cmd.Body.Value
        };
        return ExecuteWriteAsync(query, parameters);
    }

    public Task<IReadOnlyList<InboxProfileDto>> GetInboxAsync(ProfileId id)
    {
        const string query =
            """
            MATCH (p:Profile { id: $id })
            MATCH (u:Profile)
            WHERE EXISTS { MATCH (p)<-[:MESSAGE]-(u) }
               OR EXISTS { MATCH (u)<-[:MESSAGE]-(p) }
            RETURN u.id AS id,
                   u.displayName AS displayName
            """;
        return ExecuteQueryAsync(query, new { id = id.Value }, MapInboxProfile);
    }

    public IAsyncEnumerable<MessageDto> GetMessagesAsync(string p1, string p2)
    {
        const string query =
            """
            MATCH (p1:Profile{id:$p1})
            MATCH (p2:Profile{id:$p2})
            MATCH (from:Profile)-[m:MESSAGE]->(to:Profile)
            WHERE (from = p1 AND to = p2) OR (from = p2 AND to = p1)
            ORDER BY m.sentAt
            RETURN m.id as id,
                   from.displayName AS from,
                   to.displayName AS to,
                   m.sentAt as sentAt,
                   m.body as body
            """;
        return ExecuteReaderQueryAsync(query, new { p1, p2 }, MapMessage);
    }

    MessageDto MapMessage(IRecord record)
    {
        var id = new MessageId(Guid.Parse(record["id"].As<string>()));
        var from = new ProfileId(record["from"].As<string>());
        var to = new ProfileId(record["to"].As<string>());
        var sentAt = record["sentAt"].As<DateTimeOffset>();
        var body = new MessageBody(record["body"].As<string>());
        return new(id, from, to, sentAt, body);
    }

    InboxProfileDto MapInboxProfile(IRecord record)
    {
        var id = new ProfileId(record["id"].As<string>());
        var displayName = new DisplayName(record["displayName"].As<string>());
        return new (id, displayName);
    }
}