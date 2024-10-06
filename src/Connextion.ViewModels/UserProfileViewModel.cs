namespace Connextion.ViewModels;

public class UserProfileViewModel(IProfileRepository profileRepository)
{
    public string FullName { get; private set; } = "";
    public IReadOnlyList<PostViewModel> LatestPosts { get; private set; } = [];
    public IReadOnlyCollection<User> Followeing { get; private set; } = [];
    public IReadOnlyCollection<User> Followers { get; private set; } = [];
    
    public string ContactStatus { get; private set; } = "";

    public async Task InitializeAsync(string profileUser, UserDetails currentUser)
    {
        var profile = await profileRepository.GetProfileAsync(profileUser).ConfigureAwait(false);
        FullName = profile.FullName;
        LatestPosts = profile.LatestPosts.Select(post => new PostViewModel(post)).ToArray();
        Followeing = profile.Following;
        Followers = profile.Followers;
    } 
}