namespace Connextion.ViewModels.Profiles;

public class BioViewModel
{
    private readonly ProfileService? _profileService;
    private readonly User? _currentUser;
    
    public BioViewModel(
        ProfileService? profileService,
        User? currentUser,
        ProfileSummary profile)
    {
        _profileService = profileService;
        _currentUser = currentUser;
        CanEdit = profileService is not null && profile.Id == currentUser?.Id;
        Text = profile.Bio.Value;
    }

    public BioViewModel(ProfileSummary profile) : this(null, null, profile) 
    {
    }
    
    public string Text { get; private set; }
    public bool CanEdit { get; private set; }
    public bool ShowEdit { get; private set; }
    public string EditText { get; set; } = "";
    public bool IsBusy { get; private set; }

    public void Edit()
    {
        EditText = Text;
        ShowEdit = true;
    }

    public void CancelEdit()
    {
        ShowEdit = false;
    }

    public async Task SaveAsync()
    {
        if (_profileService is null || _currentUser is null) return;
        IsBusy = true;
        (Text, ShowEdit) = await _profileService
            .UpdateBioAsync(_currentUser, EditText)
            .MapAsync(() => (EditText, false))
            .DefaultAsync(_ => (Text, true))
            .ConfigureAwait(false);
        IsBusy = false;
    }
}