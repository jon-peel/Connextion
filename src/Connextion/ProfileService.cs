namespace Connextion;

public class ProfileService(IProfileRepository profileRepository)
{
    public async Task<Result> FollowAsync(User currentUser, Profile toFollow)
    {
        var eventResult = await currentUser.FollowAsync(toFollow).ConfigureAwait(false);
        var result = await eventResult.BindAsync(profileRepository.FollowAsync).ConfigureAwait(false);
        return result;
    }
}