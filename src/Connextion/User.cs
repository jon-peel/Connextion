namespace Connextion;

public record User(string Username, string FullName, int Degrees = 0);
public record CreateUserCmd(string Username, string FullName);


public interface IUserRepository
{
    public Task<User[]> GetUsernamesAsync();

    public Task CreateUserAsync(CreateUserCmd cmd);
}

