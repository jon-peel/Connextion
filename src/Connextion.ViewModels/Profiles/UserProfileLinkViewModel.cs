using Connextion.OldD;

namespace Connextion.ViewModels.Profiles;

public class UserProfileLinkViewModel(MiniProfile user)
{
    public string FullName { get; } = user.FullName;
    public string Url { get; } = $"/profile/{user.Username}";
    public int Degrees { get; } = user.Degrees;
}

