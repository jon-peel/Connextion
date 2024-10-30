

namespace Connextion.ViewModels.Profiles;

public class ProfileLinkViewModel(ProfileSummary user)
{
    public string DisplayName { get; } = user.DisplayName.Value;
    public string Url { get; } = $"/profile/{user.Id.Value}";
    public byte Degrees { get; private set; }
    public BioViewModel Bio { get; } = new (user);
    public string Id { get; } = user.Id.Value;

    public async Task InitializeAsync(User currentUser) => 
        Degrees = await user.GetDegreesFromAsync(currentUser.Id).ConfigureAwait(false);
}

