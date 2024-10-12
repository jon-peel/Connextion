using Connextion.OldD;

namespace Connextion.ViewModels.Profiles;

public class RelationshipBodyViewModel(ProfileService profileService, OldD.Profile profile, User user)
{
    public bool IsBusy { get; private set; } = false;
    public string Description { get; private set; } = CreateDescription(profile, user);
    public bool CanFollow { get; private set; } = profile.Followers.All(x => x.Username != user.Username);
    
    static string CreateDescription(OldD.Profile profile, User user)
    {
        var follows = profile.Followers.Any(x => x.Username == user.Username);
        var followed = profile.Following.All(x => x.Username == user.Username);

        return (follows, follwed: followed) switch
        {
            (true, true) => "Mutual",
            (true, _) => $"You follow {profile.User.DisplayName}",
            (_, true) => $"{profile.User.DisplayName} follows you",
            _ => "No following",
        };
    }

    public async Task FollowAsync()
    {
        IsBusy = true;
        CanFollow = false;
        await profileService.FollowAsync(user, profile.User.Username).ConfigureAwait(false);
        Description = "Followed";
        IsBusy = false;
    }
}