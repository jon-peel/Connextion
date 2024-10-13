namespace Connextion;

public record CreateUserCmd(string Username, string DisplayName);

public interface IUserRepository 
{
    IAsyncEnumerable<ProfileSummary> GetAllUsersAsync();
    Task<Result> CreateUserAsync(CreateUserCmd cmd);
}

public class User : Profile
{
    internal User(ProfileId username, 
        DisplayName displayName, 
        IAsyncEnumerable<Post> posts, 
        IAsyncEnumerable<ProfileSummary> following, 
        IAsyncEnumerable<ProfileSummary> followers) : base(username, displayName, posts, following, followers)
    { }
    
    public ProfileId UserName => Id;
    
    public Result<PostCreated> CreatePost(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return Result<PostCreated>.Error("body cannot be empty");
        if (body.Length > 500) return Result<PostCreated>.Error("body cannot be longer than 500 characters");

        var id = new PostId(Guid.NewGuid());
        var created = new PostCreated(id, Id, DateTime.UtcNow, new(body));
        return created;
    }

    public async Task<Result<FollowCmd>> FollowAsync(Profile toFollow)
    {
        var canFollow = await CanFollowAsync(toFollow);
        return canFollow.Map(() => new FollowCmd(Id.Value, toFollow.Id.Value));
    }

    async Task<Result> CanFollowAsync(Profile toFollow)
    {
        if (toFollow.Id.Value == Id.Value) return Result.Error("can't follow yourself");
        var alreadyFollows = await Following.AnyAsync(x => x.Id == toFollow.Id);
        return alreadyFollows ? Result.Error("already following") : Result.Ok();
    }
}