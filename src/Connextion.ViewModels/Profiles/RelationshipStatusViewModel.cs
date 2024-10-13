using Connextion.OldD;

namespace Connextion.ViewModels.Profiles;

public class RelationshipStatusViewModel(ProfileService profileService, Profile currentUser, Profile profile)
{
    public bool IsBusy { get; private set; } = false;
    public string Description { get; private set; } = CreateDescription(currentUser, profile);
    public bool CanFollow { get; private set; }


    public async Task InitializeAsync()
    {
        CanFollow = await currentUser.CanFollowAsync(profile).ConfigureAwait(false);
    }
    
    
    static string CreateDescription(Profile currentUser, Profile profile)
    {
        return "";
        //TODO: 
        // var follows = profile.Followers.Any(x => x.Username == user.Username);
        // var followed = profile.Following.All(x => x.Username == user.Username);
        //
        // return (follows, follwed: followed) switch
        // {
        //     (true, true) => "Mutual",
        //     (true, _) => $"You follow {profile.User.DisplayName}",
        //     (_, true) => $"{profile.User.DisplayName} follows you",
        //     _ => "No following",
        // };
    }

    public async Task FollowAsync()
    {
        IsBusy = true;
        CanFollow = false;
        await profileService.FollowAsync(currentUser, profile).ConfigureAwait(false);
        Description = "Followed";
        IsBusy = false;
    }
}