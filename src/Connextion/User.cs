namespace Connextion;

public record CreateUserCmd(string Username, string DisplayName, string Bio);
public record UpdateBioCmd(string ProfileId, string Bio);

public interface IUserRepository 
{
    IAsyncEnumerable<ProfileSummary> GetAllUsersAsync();
    Task<Result> CreateUserAsync(CreateUserCmd cmd);
    Task<User> GetUserAsync(string id);
}

public class User : Profile
{
    public User(ProfileId username, 
        DisplayName displayName, 
        Bio bio,
        IAsyncEnumerable<Post> posts, 
        IAsyncEnumerable<ProfileSummary> following, 
        IAsyncEnumerable<ProfileSummary> followers) : base(username, displayName, bio, posts, following, followers)
    { }
    
    public ProfileId UserName => Id;
    
    public Result<PostCreated> CreatePost(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return Result<PostCreated>.Error("body cannot be empty");
        if (body.Length > 500) return Result<PostCreated>.Error("body cannot be longer than 500 characters");

        var id = new PostId(Guid.NewGuid());
        var created = new PostCreated(id, Id, DateTime.UtcNow, new(body));
        return created.ToResult();
    }

    public async Task<Result<FollowCmd>> FollowAsync(Profile toFollow)
    {
        var canFollow = await CanFollowAsync(toFollow);
        return canFollow.Map(() => new FollowCmd(Id.Value, toFollow.Id.Value));
    }

    public async Task<Result> CanFollowAsync(Profile toFollow)
    {
        if (toFollow.Id.Value == Id.Value) return Result.Error("can't follow yourself");
        var alreadyFollows = await Following.AnyAsync(x => x.Id == toFollow.Id);
        return alreadyFollows ? Result.Error("already following") : Result.Ok();
    }

    public Result CanSetBio(string text)
    {
        if (Bio.Value.Equals(text)) return Result.Error("nothing to update");
        if (string.IsNullOrWhiteSpace(text)) return Result.Error("bio must be set");
        if (text.Length > 2000) return Result.Error("bio cannot be longer than 2000 characters");
        return Result.Ok();
    }
    
    public Result<UpdateBioCmd> UpdateBio(string text) => 
        CanSetBio(text)
            .Map(() => new UpdateBioCmd(Id.Value, text));

    public async Task<bool> CanMessageAsync(ProfileId profileId)
    {
        var followed = await Followers.AnyAsync(x => x.Id == profileId).ConfigureAwait(false);
        if (!followed) return false;
        var following = await Following.AnyAsync(x => x.Id == profileId).ConfigureAwait(false);
        return following && followed;
    }
}