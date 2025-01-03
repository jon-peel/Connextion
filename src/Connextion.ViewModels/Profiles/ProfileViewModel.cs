

namespace Connextion.ViewModels.Profiles;

public class ProfileViewModel(PostService postService, ProfileService profileService, IProfileRepository profileRepository)
{
    string _profileUser = "";

    public bool IsBusy { get; private set; } = true;
    public string DisplayName { get; private set; } = "";
    public BioViewModel? Bio { get; private set; }
    public IReadOnlyList<PostViewModel> LatestPosts { get; private set; } = [];
    public IReadOnlyList<ProfileLinkViewModel> Following { get; private set; } = [];
    public IReadOnlyList<ProfileLinkViewModel> Followers { get; private set; } = [];
    public RelationshipStatusViewModel? RelationshipStatus { get; private set; }
    public DirectMessageLinkViewModel? DirectMessageLink { get; private set; }


    public async Task InitializeAsync(string profileUser, User currentUser)
    {
        if (_profileUser == profileUser) return;
        IsBusy = true;
        _profileUser = profileUser;
        var profile = await profileRepository
            .GetProfileAsync(profileUser)
            .ConfigureAwait(false);
        RelationshipStatus = new (profileService, currentUser, profile);
        DirectMessageLink = new(profileUser, currentUser);
        DisplayName = profile.DisplayName.Value;
        Bio = new (profileService, currentUser, profile);
        
        LatestPosts = await profile.Posts.Take(10).Select(post => new PostViewModel(postService, post, currentUser)).ToArrayAsync().ConfigureAwait(false);
        Following = await profile.Following.Select(u => new ProfileLinkViewModel(u)).ToArrayAsync().ConfigureAwait(false);
        Followers = await profile.Followers.Select(u => new ProfileLinkViewModel(u)).ToArrayAsync().ConfigureAwait(false);
        
        IsBusy = false;
    }
}