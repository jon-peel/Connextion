using Connextion.Events;

namespace Connextion.ViewModels.Events;

public class CreateEventViewModel(EventService eventService)
{
    User? _currentUser;

    public bool IsBusy { get; private set; }
    public string? Error { get; private set; }

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Capacity { get; set; }
    public bool MultiDay { get; set; }
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly EndDate { get; set; }= DateOnly.FromDateTime(DateTime.Today);
    
    
    
    public void Initialize(User currentUser) => _currentUser = currentUser;

    public async Task<Result<EventName>> CreateEventAsync()
    {
        IsBusy = true;
        if (!MultiDay) EndDate = StartDate;
        var capacity = (ushort)Capacity;
        var (r, e) = await eventService
            .CreateEventAsync(Name, Description, _currentUser!, capacity, StartDate, EndDate)
            .MapAsync(r => ((EventName?)r, default(string)))
            .DefaultAsync(e => (null, e))
            .ConfigureAwait(false);
        Error = e;
        IsBusy = false;
        return r is not null ? r.ToResult() : Result<EventName>.Error(e!);
    } 
    
}