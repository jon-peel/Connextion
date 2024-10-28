using Connextion.Events;

namespace Connextion.ViewModels.Events;

public class EventsViewModel(IEventRepository eventRepository)
{
    public Tab Show { get; set; } = Tab.Available;

    public IReadOnlyList<EventDetailsDto> AttendingEvents { get; private set; } = Array.Empty<EventDetailsDto>();
    public IReadOnlyList<EventDetailsDto> OrganisingEvents { get; private set; } = Array.Empty<EventDetailsDto>();
    public IReadOnlyList<EventDetailsDto> AvailableEvents { get; private set; } = Array.Empty<EventDetailsDto>();
    public bool IsBusy { get; private set; } = true;


    public async Task InitializeAsync(User currentUser)
    {
        var result = await eventRepository
            .GetAllEventsAsync(currentUser.Id, DateOnly.FromDateTime(DateTime.Today))
            .ConfigureAwait(false);
        OrganisingEvents = result.OrganisingEvents;
        AttendingEvents = result.AttendingEvents;
        AvailableEvents = result.AvailableEvents;
        IsBusy = false;
    }

    public enum Tab
    {
        Available,
        Organising,
        Attending
    }
}