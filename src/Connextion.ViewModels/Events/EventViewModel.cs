using Connextion.Events;

namespace Connextion.ViewModels.Events;

public class EventViewModel(IEventRepository eventRepository)
{
    public bool IsBusy { get; private set; } = true;
    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    public bool MultiDay { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public IReadOnlyList<ProfileSummary> Organisers { get; private set; } = Array.Empty<ProfileSummary>();
    public IReadOnlyList<ProfileSummary> Attendees { get; private set; } = Array.Empty<ProfileSummary>();

    public async Task InitializeAsync(User currentUser, string key)
    {
        var e = await eventRepository.GetEventsAsync(key).ConfigureAwait(false);
        if (e is null) return;
        Name = e.Name.FullName;
        Description = e.Description.Value;
        Organisers = e.Organisers.People.ToArray();
        Attendees = e.Attendees.People.ToArray();

        (MultiDay, StartDate, EndDate) = e switch
        {
            SingleDayEvent sde => (false, (DateOnly?)sde.Date, default(DateOnly?)),
            MultiDayEvent mde => (true, mde.StartDate, mde.EndDate),
            _ => (false, null, null)
        };
        IsBusy = false;
    }
}