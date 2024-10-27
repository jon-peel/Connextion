namespace Connextion.Events;

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
    Task<AllEventsDto> GetAllEventsAsync(ProfileId profileId, DateOnly date);
}

public class EventService(IEventRepository eventRepository)
{
    public Task<Result<EventName>> CreateEventAsync(string name, string description, ProfileSummary createdBy, ushort capacity, DateOnly startDate, DateOnly endDate) =>
        Event
            .CreateCommand(name, description, createdBy, capacity, startDate, endDate)
            .BindAsync(eventRepository.CreateEventAsync);
}