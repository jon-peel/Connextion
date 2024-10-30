using Connextion.Events;
using Connextion.ViewModels.Profiles;

namespace Connextion.ViewModels.Events;

public class EventViewModel(EventService eventService, IEventRepository eventRepository)
{
    string? _key;
    User? _currentUser;
    
    public bool IsBusy { get; private set; } = true;
    public string? AttendeeError { get; private set; }
    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    public bool MultiDay { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public IReadOnlyList<ProfileLinkViewModel> Organisers { get; private set; } = Array.Empty<ProfileLinkViewModel>();
    public IReadOnlyList<ProfileLinkViewModel> Attendees { get; private set; } = Array.Empty<ProfileLinkViewModel>();
    public bool CanAddOrganiser { get; private set; }
    public Tab Show { get; set; }
    

    public async Task InitializeAsync(User currentUser, string key)
    {
        _key = key;
        _currentUser = currentUser; 
        await GetEventDetailsAsync().ConfigureAwait(false);
        IsBusy = false;
    }

    async Task GetEventDetailsAsync()
    {
        if (_key is null) return;
        var e = await eventRepository.GetEventAsync(_key).ConfigureAwait(false);
        if (e is null) return;
        Name = e.Name.FullName;
        Description = e.Description.Value;
        Organisers = e.Organisers.People.Select(x => new ProfileLinkViewModel(x)).ToArray();
        Attendees = e.Attendees.People.Select(x => new ProfileLinkViewModel(x)).ToArray();
        CanAddOrganiser = e.Organisers.People.Any(x => x.Id == _currentUser?.Id);

        (MultiDay, StartDate, EndDate) = e switch
        {
            SingleDayEvent sde => (false, (DateOnly?)sde.Date, default(DateOnly?)),
            MultiDayEvent mde => (true, mde.StartDate, mde.EndDate),
            _ => (false, null, null)
        };
    }

    public async Task AttendAsync()
    {
        if (string.IsNullOrEmpty(_key) || _currentUser is null) return;
        IsBusy = true;
        AttendeeError = await eventService
            .AttendEventAsync(_key, _currentUser.Id)
            .MapAsync(() => default(string))
            .DefaultAsync(e => e)
            .ConfigureAwait(false);
        await GetEventDetailsAsync().ConfigureAwait(false);
        IsBusy = false;
    }
    
    public enum Tab { Attendees, Organisers }

    public async Task AddOrganiserAsync(string id)
    {
        if (string.IsNullOrEmpty(_key)) return;
        IsBusy = true;
        AttendeeError = await eventService
            .AddOrganiserAsync(_key, id)
            .MapAsync(() => "Success")
            .DefaultAsync(x => x)
            .ConfigureAwait(false);
        IsBusy = false;
    }
}