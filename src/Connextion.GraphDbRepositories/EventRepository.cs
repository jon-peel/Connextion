using Connextion.Events;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public class EventRepository(IDriver driver) : RepositoryBase(driver), IEventRepository
{
    public Task<Result<EventName>> CreateEventAsync(CreateEventCmd cmd)
    {
        const string query =
            """
            MATCH (createdBy:Profile { id: $CreatedById })
            CREATE (event:Event)
            SET event.id = $Id
            SET event.key = $Key
            SET event.name = $Name
            SET event.description = $Description
            SET event.capacity = $Capacity
            SET event.startDate = $StartDate
            SET event.endDate = $EndDate
            CREATE (event)-[:ORGANISED_BY]->(createdBy)
            CREATE (event)-[:ATTENDED_BY]->(createdBy)
            """;
        var parameters = new
        {
            cmd.Id, cmd.Key, cmd.Name, cmd.Description, cmd.CreatedById, Capacity = (int)cmd.Capacity, cmd.StartDate, cmd.EndDate
        };
        return ExecuteWriteAsync(query, parameters)
            .MapAsync(() => new EventName(cmd.Key, cmd.Name));
    }

    public async Task<AllEventsDto> GetAllEventsAsync(ProfileId profileId, DateOnly date)
    {
        const string query =
            """
            WITH $date AS date
            MATCH (p:Profile { id: $profileId })
            RETURN 
                COLLECT { 
                    MATCH (e:Event)-[:ORGANISED_BY]->(p) WHERE e.startDate > date
                    RETURN { key: e.key,
                             name: e.name,
                             description: e.description,
                             capacity: e.capacity,
                             attendees: COUNT{ (e)-[:ATTENDED_BY]->(a) }, 
                             startDate: e.startDate } AS D } AS organising,
                COLLECT { 
                    MATCH (e:Event)-[:ATTENDED_BY]->(p) WHERE e.startDate > date 
                    RETURN { key: e.key,
                             name: e.name,
                             description: e.description,
                             capacity: e.capacity,
                             attendees: COUNT{ (e)-[:ATTENDED_BY]->(a) }, 
                             startDate: e.startDate } AS D } AS attending,
                COLLECT { 
                    MATCH (e:Event) WHERE e.startDate > date 
                    RETURN { key: e.key,
                             name: e.name,
                             description: e.description,
                             capacity: e.capacity,
                             attendees: COUNT{ (e)-[:ATTENDED_BY]->(a) }, 
                             startDate: e.startDate } AS D } AS available
            """;
        var parameters = new { profileId = profileId.Value, date };
        var results = await ExecuteQueryAsync(query, parameters, MapAllEventsDto).ConfigureAwait(false);
        return results.Single();
    }

    AllEventsDto MapAllEventsDto(IRecord record)
    {
        var organising = record["organising"].As<IEnumerable<IReadOnlyDictionary<string, object>>>().Select(MapEventDetailsDto).ToArray();
        var attending = record["attending"].As<IEnumerable<IReadOnlyDictionary<string, object>>>().Select(MapEventDetailsDto).ToArray();
        var available = record["available"].As<IEnumerable<IReadOnlyDictionary<string, object>>>().Select(MapEventDetailsDto).ToArray();
        return new(organising, attending, available);
    }

    EventDetailsDto MapEventDetailsDto(IReadOnlyDictionary<string, object> record){
        var key = record["key"].As<string>();
        var name = record["name"].As<string>();
        var description = record["description"].As<string>();
        var capacity = (ushort) record["capacity"].As<int>();
        var attendees = (ushort) record["attendees"].As<int>();
        var startDate = record["startDate"].As<DateOnly>();
        return new(key, name, description, capacity, attendees, startDate); 
    }
}