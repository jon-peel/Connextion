namespace Connextion.ViewModels.Profiles;

public class BioViewModel
{
    private readonly ProfileService _profileService;
    private readonly User _currentUser;
    
    public BioViewModel(
        ProfileService profileService, 
        ProfileSummary profile,
        User currentUser,
        bool editable)
    {
        _profileService = profileService;
        _currentUser = currentUser;
        CanEdit = editable && profile.Id == currentUser.Id;
        Text = profile.Bio.Value;
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
        (Text, ShowEdit) = await _profileService
            .UpdateBioAsync(_currentUser, EditText)
            .MapAsync(() => (EditText, false))
            .DefaultAsync(_ => (Text, true))
            .ConfigureAwait(false);
    }
}