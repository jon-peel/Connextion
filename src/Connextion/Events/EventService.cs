namespace Connextion.Events;

public record AddAttendeeCmd(string EventId, string AttendeeId);
    
public record CreateEventCmd(
    string Id,
    string Key,
    string Name,
    string Description,
    string CreatedById,
    ushort Capacity,
    DateOnly StartDate,
    DateOnly EndDate);

public record EventDetailsDto(string Key, string Name, string Description, ushort Capacity, ushort Attendees, DateOnly StartDate);
public record AllEventsDto(
    IReadOnlyList<EventDetailsDto> OrganisingEvents, 
    IReadOnlyList<EventDetailsDto> AttendingEvents, 
    IReadOnlyList<EventDetailsDto> AvailableEvents);

public interface IEventRepository
{
    Task<Result<EventName>> CreateEventAsync(CreateEventCmd cmd);
    Task<Result> AddAttendeeAsync(AddAttendeeCmd cmd);
    Task<AllEventsDto> GetAllEventsAsync(ProfileId profileId, DateOnly date);
    Task<Event?> GetEventAsync(string key);
}

public class EventService(IEventRepository eventRepository)
{
    public Task<Result<EventName>> CreateEventAsync(string name, string description, ProfileSummary createdBy, ushort capacity, DateOnly startDate, DateOnly endDate) =>
        Event
            .CreateCommand(name, description, createdBy, capacity, startDate, endDate)
            .BindAsync(eventRepository.CreateEventAsync);

    public async Task<Result> AttendEventAsync(string key, ProfileId currentUserId)
    {
        var e = await eventRepository.GetEventAsync(key).ConfigureAwait(false);
        if (e is null) return Result.Error("Event not found");
        return await e.AddAttendee(currentUserId).BindAsync(eventRepository.AddAttendeeAsync).ConfigureAwait(false);
    }
}