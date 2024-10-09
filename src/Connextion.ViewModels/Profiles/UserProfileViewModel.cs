namespace Connextion.ViewModels.Profiles;

public class UserProfileViewModel(ProfileService profileService, IProfileRepository profileRepository)
{
    string _profileUser = "";

    public bool IsBusy { get; private set; } = true;
    public string FullName { get; private set; } = "";
    public IReadOnlyList<PostViewModel> LatestPosts { get; private set; } = [];
    public IReadOnlyList<UserProfileLinkViewModel> Following { get; private set; } = [];
    public IReadOnlyList<UserProfileLinkViewModel> Followers { get; private set; } = [];
    public RelationshipStatusViewModel? RelationshipStatus { get; private set; }

    public async Task InitializeAsync(string profileUser, UserDetails currentUser)
    {
        if (_profileUser == profileUser) return;
        IsBusy = true;
        _profileUser = profileUser;
        var profile = await profileRepository.GetProfileAsync(profileUser, currentUser.username).ConfigureAwait(false);
        RelationshipStatus = new RelationshipStatusViewModel(profileService, profile, currentUser);
        FullName = profile.User.FullName;
        LatestPosts = profile.LatestPosts.Select(post => new PostViewModel(post)).ToArray();
        Following = profile.Following.Select(u => new UserProfileLinkViewModel(u)).ToArray();
        Followers = profile.Followers.Select(u => new UserProfileLinkViewModel(u)).ToArray();
        IsBusy = false;
    } 
}