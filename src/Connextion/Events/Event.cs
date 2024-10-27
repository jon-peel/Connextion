namespace Connextion.Events;

public record EventId(Guid Value);

public record EventName(string Key, string FullName);

public record EventDescription(string Value);

public record EventOrganisers(IReadOnlySet<ProfileSummary> People);

public record EventAttendees(ushort Capacity, IReadOnlySet<ProfileSummary> People);

public abstract class Event
{
    protected Event(EventId id, EventName name, EventDescription description, EventOrganisers organisers,
        EventAttendees eventAttendees)
    {
        Id = id;
        Name = name;
        Description = description;
        Organisers = organisers;
        EventAttendees = eventAttendees;
    }

    EventId Id { get; }
    EventName Name { get; }
    EventDescription Description { get; }
    EventOrganisers Organisers { get; }
    EventAttendees EventAttendees { get; }

    public static Result<CreateEventCmd> CreateCommand(
        string name,
        string description,
        ProfileSummary createdBy,
        ushort capacity,
        DateOnly startDate,
        DateOnly endDate) 
        => Create(name, description, createdBy, capacity, startDate, endDate)
            .Bind(e => e.AsCreateCommand());

    static Result<Event> Create(
        string name,
        string description,
        ProfileSummary createdBy,
        ushort capacity,
        DateOnly startDate,
        DateOnly endDate)
    {
        var id = new EventId(Guid.NewGuid());

        if (string.IsNullOrWhiteSpace(name)) return Result<Event>.Error("name cannot be empty");
        if (name.Length > 100) return Result<Event>.Error("name cannot be longer than 100 characters");
        if (string.IsNullOrWhiteSpace(description)) return Result<Event>.Error("description cannot be empty");
        if (description.Length > 1000) return Result<Event>.Error("description cannot be longer than 1000 characters");
        if (capacity < 1) return Result<Event>.Error("capacity must be greater than 0");
        if (startDate > endDate) return Result<Event>.Error("start date cannot be after end date");

        var keyName = name.ToLower().Replace(" ", "-");
        var nameValue = new EventName(keyName, name);
        var people = new[] { createdBy }.ToHashSet();
        var organisers = new EventOrganisers(people);
        var attendees = new EventAttendees(capacity, people);

        Event result = startDate == endDate
            ? new SingleDayEvent(id, nameValue, new(description), organisers, attendees, startDate)
            : new MultiDayEvent(id, nameValue, new(description), organisers, attendees, startDate, endDate);
        return result.ToResult();
    }

    Result<CreateEventCmd> AsCreateCommand() => 
        (this switch
        {
            SingleDayEvent sde => (sde.Date, sde.Date).ToResult(),
            MultiDayEvent mde => (mde.StartDate, mde.EndDate).ToResult(),
            _ => Result<(DateOnly, DateOnly)>.Error("Something went wrong")
        })
        .Map<CreateEventCmd>(d => new(
            Id.Value.ToString(),
            Name.Key,
            Name.FullName,
            Description.Value,
            Organisers.People.Single().Id.Value,
            EventAttendees.Capacity,
            d.Item1,
            d.Item2));

}

public class SingleDayEvent : Event
{
    internal SingleDayEvent(EventId id, EventName name, EventDescription description, EventOrganisers organisers,
        EventAttendees eventAttendees, DateOnly date)
        : base(id, name, description, organisers, eventAttendees)
    {
        Date = date;
    }

    public DateOnly Date { get; }
}

public class MultiDayEvent : Event
{
    internal MultiDayEvent(EventId id, EventName name, EventDescription description, EventOrganisers organisers,
        EventAttendees eventAttendees,
        DateOnly startDate, DateOnly endDate) : base(id, name, description, organisers, eventAttendees)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
}