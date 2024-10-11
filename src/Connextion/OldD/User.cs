namespace Connextion.OldD;


public abstract class User(string username, string displayName)
{
    public string Username { get; } = username;
    public string DisplayName { get; } = displayName;
}

public class MiniProfile(string username, string displayName, byte degrees) : User(username, displayName)
{
    public byte Degrees { get; } = degrees;
}

public record CreateUserCmd(string Username, string DisplayName);

public interface IUserRepository
{
    public Task<User[]> GetAllUsersAsync();

    public Task CreateUserAsync(CreateUserCmd cmd);
}

