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