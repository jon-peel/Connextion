namespace Connextion;

public record User(string Username, string FullName);
public record CreateUserCmd(string Username, string FullName);


public interface IUserRepository
{
    public Task<User[]> GetUsernamesAsync();

    public Task CreateUserAsync(CreateUserCmd cmd);
}

