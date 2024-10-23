namespace Connextion.ViewModels.Profiles;

public class DirectMessageLinkViewModel(string profileUser, User currentUser)
{
    public bool CanSend { get; } = GenerateCanSend(profileUser, currentUser);
    public string LinkUrl { get; } = $"/Messages/{profileUser}";

    static bool GenerateCanSend(string profileUser, User user) =>
        user.CanMessageAsync(new(profileUser)).GetAwaiter().GetResult();
}