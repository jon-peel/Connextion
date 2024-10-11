using Connextion.OldD;

namespace Connextion.ViewModels.Profiles;

public class RelationshipStatusViewModel(ProfileService profileService, OldD.Profile profile, CurrentUser currentUser)
{
    public bool IsBusy { get; private set; } = false;
    public string Description { get; private set; } = CreateDescription(profile, currentUser);
    public bool CanFollow { get; private set; } = profile.Followers.All(x => x.Username != currentUser.Username);
    
    static string CreateDescription(OldD.Profile profile, CurrentUser currentUser)
    {
        var follows = profile.Followers.Any(x => x.Username == currentUser.Username);
        var followed = profile.Following.All(x => x.Username == currentUser.Username);

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
        await profileService.FollowAsync(currentUser, profile.User.Username).ConfigureAwait(false);
        Description = "Followed";
        IsBusy = false;
    }
}