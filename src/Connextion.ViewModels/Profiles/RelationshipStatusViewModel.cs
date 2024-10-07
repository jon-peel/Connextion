namespace Connextion.ViewModels.Profiles;

public class RelationshipStatusViewModel(ProfileService profileService, Profile profile, UserDetails currentUser)
{
    public bool IsBusy { get; private set; } = false;
    public string Description { get; private set; } = CreateDescription(profile, currentUser);
    public bool CanFollow { get; private set; } = profile.Followers.All(x => x.Username != currentUser.username);
    
    static string CreateDescription(Profile profile, UserDetails currentUser)
    {
        var follows = profile.Followers.Any(x => x.Username == currentUser.username);
        var followed = profile.Following.All(x => x.Username == currentUser.username);

        return (follows, follwed: followed) switch
        {
            (true, true) => "Mutual",
            (true, _) => $"You follow {profile.User.FullName}",
            (_, true) => $"{profile.User.FullName} follows you",
            _ => "No following",
        };
    }

    public async Task FollowAsync()
    {
        IsBusy = true;
        CanFollow = false;
        await profileService.FollowAsync(currentUser.username, profile.User.Username).ConfigureAwait(false);
        Description = "Followed";
        IsBusy = false;
    }
}