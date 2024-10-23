using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public abstract class RepositoryBase(IDriver driver)
{
    protected async Task<IReadOnlyList<TOut>> ExecuteQueryAsync<TOut>(string query, object parameters, Func<IRecord, TOut> map)
    {
        var (result, _) = await driver
            .ExecutableQuery(query)
            .WithParameters(parameters)
            .WithMap(map)
            .ExecuteAsync()
            .ConfigureAwait(false);
        return result;
    }
    
    protected async IAsyncEnumerable<TOut> ExecuteReaderQueryAsync<TOut>(string query, object parameters, Func<IRecord, TOut> map)
    {
        await using var session = driver.AsyncSession();
        var reader = await session.RunAsync(query, parameters).ConfigureAwait(false);
        while (await reader.FetchAsync())
        {
            var result = map(reader.Current);
            yield return result;
        }
    }
    
    protected async Task<Result> ExecuteWriteAsync(string query, object parameters) {
        try
        {
            await driver
                .ExecutableQuery(query)
                .WithParameters(parameters)
                .ExecuteAsync()
                .ConfigureAwait(false);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Error(e.Message);
        }
    }
}

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
}