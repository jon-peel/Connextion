using Connextion.OldD;

namespace Connextion.ViewModels.Profiles;

public class UserProfileLinkViewModel(ProfileSummary user)
{
    public string DisplayName { get; } = user.DisplayName;
    public string Url { get; } = $"/profile/{user.Username}";
    public int Degrees { get; } = user.Degrees;
}

