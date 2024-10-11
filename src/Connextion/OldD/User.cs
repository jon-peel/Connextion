namespace Connextion.OldD;


public abstract class User(string username, string fullName)
{
    public string Username { get; } = username;
    public string FullName { get; } = fullName;
}

public class CurrentUser(string username, string fullName) : User(username, fullName) {}

public class MiniProfile(string username, string fullName, byte degrees) : User(username, fullName)
{
    public byte Degrees { get; } = degrees;
}

public record CreateUserCmd(string Username, string DisplayName);

public interface IUserRepository
{
    public Task<User[]> GetAllUsersAsync();

    public Task CreateUserAsync(CreateUserCmd cmd);
}

