using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class MessageRepository(IDriver driver) : RepositoryBase(driver), IMessageRepository
{
    public Task<Result> SendMessageAsync(SendMessageCmd arg)
    {
        const string query =
            """
            MATCH (from:Profile {id: $from})
            MATCH (to:Profile {id: $to})
            CREATE (from)-[m:MESSAGE]->(to)
            SET m.sentAt = $sentAt
            SET m.body = $body
            """;
        var parameters = new
        {
            from = arg.From.Value,
            to = arg.To.Value,
            sentAt = arg.SentAt,
            body = arg.Body.Value
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

    InboxProfileDto MapInboxProfile(IRecord arg)
    {
        var id = new ProfileId(arg["id"].As<string>());
        var displayName = new DisplayName(arg["displayName"].As<string>());
        return new (id, displayName);
    }
}