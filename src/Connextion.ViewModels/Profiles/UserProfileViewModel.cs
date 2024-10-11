using Connextion.OldD;

namespace Connextion.ViewModels.Profiles;

public class UserProfileViewModel(ProfileService profileService, IProfileRepositoryOld profileRepositoryOld)
{
    string _profileUser = "";

    public bool IsBusy { get; private set; } = true;
    public string DisplayName { get; private set; } = "";
    public IReadOnlyList<PostViewModel> LatestPosts { get; private set; } = [];
    public IReadOnlyList<UserProfileLinkViewModel> Following { get; private set; } = [];
    public IReadOnlyList<UserProfileLinkViewModel> Followers { get; private set; } = [];
    public RelationshipStatusViewModel? RelationshipStatus { get; private set; }

    public async Task InitializeAsync(string profileUser, User user)
    {
        if (_profileUser == profileUser) return;
        IsBusy = true;
        _profileUser = profileUser;
        var profile = await profileRepositoryOld.GetProfileAsync(profileUser, user).ConfigureAwait(false);
        RelationshipStatus = new RelationshipStatusViewModel(profileService, profile, user);
        DisplayName = profile.User.DisplayName;
        LatestPosts = profile.LatestPosts.Select(post => new PostViewModel(post)).ToArray();
        Following = profile.Following.Select(u => new UserProfileLinkViewModel(u)).ToArray();
        Followers = profile.Followers.Select(u => new UserProfileLinkViewModel(u)).ToArray();
        IsBusy = false;
    } 
}