using Connextion.Events;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

class EventRepository(UserRepository userRepository, IDriver driver) : RepositoryBase(driver), IEventRepository
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
            cmd.Id, cmd.Key, cmd.Name, cmd.Description, cmd.CreatedById, Capacity = (int)cmd.Capacity, cmd.StartDate,
            cmd.EndDate
        };
        return ExecuteWriteAsync(query, parameters)
            .MapAsync(() => new EventName(cmd.Key, cmd.Name));
    }

    public Task<Result> AddAttendeeAsync(AddAttendeeCmd cmd)
    {
        const string query =
            """
            MATCH (e:Event { id: $eventId })
            MATCH (a:Profile { id: $attendeeId })
            CREATE (e)-[:ATTENDED_BY]->(a)
            """;
        var parameters = new { eventId= cmd.EventId, attendeeId= cmd.AttendeeId };
        return ExecuteWriteAsync(query, parameters);
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

    public async Task<Event?> GetEventAsync(string key)
    {
        const string query =
            """
            MATCH (event:Event { key: $key })
            RETURN 
                event.id AS id,
                event.key AS key,
                event.name AS name,
                event.description AS description,
                event.capacity AS capacity,
                event.startDate AS startDate,
                event.endDate AS endDate,
                COLLECT { MATCH (event)-[:ORGANISED_BY]->(p) RETURN { id: p.id, displayName: p.displayName, bio: p.bio } AS p } AS organisers,
                COLLECT { MATCH (event)-[:ATTENDED_BY]->(p) RETURN { id: p.id, displayName: p.displayName, bio: p.bio } AS p } AS attendees
            """;
        var parameters = new { key };
        var results = await ExecuteQueryAsync(query, parameters, MapEvent).ConfigureAwait(false);
        return results.SingleOrDefault();
    }

    public Task<Result> AddOrganiserAsync(AddOrganiser cmd)
    {
        const string query =
            """
            MATCH (e:Event { id: $eventId })
            MATCH (a:Profile { id: $attendeeId })
            CREATE (e)-[:ORGANISED_BY]->(a)
            """;
        var parameters = new { eventId= cmd.EventId, attendeeId= cmd.AttendeeId };
        return ExecuteWriteAsync(query, parameters);
    }

    Event MapEvent(IRecord record)
    {
        var id = new EventId(Guid.Parse(record["id"].As<string>()));
        var name = new EventName(record["key"].As<string>(), record["name"].As<string>());
        var description = new EventDescription(record["description"].As<string>());
        var startDate = DateOnly.FromDateTime(record["startDate"].As<DateTime>());
        var endDate = DateOnly.FromDateTime(record["endDate"].As<DateTime>());
        var capacity = (ushort)record["capacity"].As<int>();

        var organisers =
            new EventOrganisers(
                record["organisers"]
                    .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
                    .Select(userRepository.MapProfileSummary)
                    .ToArray());
        var attendees =
            new EventAttendees(
                capacity,
                record["attendees"]
                    .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
                    .Select(userRepository.MapProfileSummary)
                    .ToArray());

        return startDate == endDate
            ? new SingleDayEvent(id, name, description, organisers, attendees, startDate)
            : new MultiDayEvent(id, name, description, organisers, attendees, startDate, endDate);
    }

    AllEventsDto MapAllEventsDto(IRecord record)
    {
        var organising = record["organising"].As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(MapEventDetailsDto).ToArray();
        var attending = record["attending"].As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(MapEventDetailsDto).ToArray();
        var available = record["available"].As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(MapEventDetailsDto).ToArray();
        return new(organising, attending, available);
    }

    EventDetailsDto MapEventDetailsDto(IReadOnlyDictionary<string, object> record)
    {
        var key = record["key"].As<string>();
        var name = record["name"].As<string>();
        var description = record["description"].As<string>();
        var capacity = (ushort)record["capacity"].As<int>();
        var attendees = (ushort)record["attendees"].As<int>();
        var startDate = DateOnly.FromDateTime(record["startDate"].As<DateTime>());
        return new(key, name, description, capacity, attendees, startDate);
    }
}