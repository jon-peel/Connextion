using Connextion.OldD;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

internal class ConfigureTheDatabase(
    IDriver driver,
    IUserRepository userRepository,
    IProfileRepositoryOld profileRepositoryOld,
    IPostRepositoryOld old)
{
    public async Task RunAsync()
    {
        await ConfigureConstraintsAsync().ConfigureAwait(false);
        var users = await userRepository.GetAllUsersAsync().ConfigureAwait(false);
        if (users.Length != 0) return;
        await ConfigureDefaultUsers().ConfigureAwait(false);
    }

    async Task ConfigureConstraintsAsync()
    {
        string[] constraints =
        [
            "CREATE CONSTRAINT IF NOT EXISTS FOR (user:User) REQUIRE user.username IS UNIQUE",
            "CREATE CONSTRAINT IF NOT EXISTS FOR (profile:Profile) REQUIRE profile.id IS UNIQUE",
            "CREATE CONSTRAINT IF NOT EXISTS FOR (post:Post) REQUIRE post.id IS UNIQUE",
            "CREATE INDEX IF NOT EXISTS FOR (p:Post) ON (p.postedAt)"
        ];
        await using var session = driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            foreach (var constraint in constraints)
                await tx.RunAsync(constraint).ConfigureAwait(false);
        });
    }

    async Task ConfigureDefaultUsers()
    {
        var users = UserCommands();
        await CreateUsersAsync(users).ConfigureAwait(false);
        await FollowUsersAsync(users).ConfigureAwait(false);
        await CreatePostsAsync(users).ConfigureAwait(false);
    }

    async Task CreatePostsAsync(IReadOnlyList<CreateUserCmd> users)
    {
        var random = new Random();
        foreach (var user in users)
        {
            var numPosts = random.Next(1, 23);
            for (var i = 0; i < numPosts; i++)
            {
                var postCmd = new CreatePostCmd(
                    Username: user.Username,
                    Body: $"Body {i} from {user.DisplayName}",
                    PostedAt: CreateRandomDate()
                );
                await old.SubmitBodyAsync(postCmd).ConfigureAwait(false);
            }
        }

        return;

        DateTime CreateRandomDate()
        {
            var month = random.Next(1, 9);
            var day = random.Next(1, 28);
            var hour = random.Next(0, 23);
            var minute = random.Next(0, 59);
            return new(2024, month, day, hour, minute, 05);
        }
    }

    async Task FollowUsersAsync(IReadOnlyList<CreateUserCmd> users)
    {
        var random = new Random();
        foreach (var user in users)
        {
            var usersToFollow = users
                .Where(u => u.Username != user.Username)
                .OrderBy(_ => random.Next())
                .Take(random.Next(1, users.Count / 2))
                .Select(u => u.Username);

            foreach (var userToFollow in usersToFollow)
            {
                // Assuming you have a FollowAsync method in your UserRepository
                await profileRepositoryOld.FollowAsync(user.Username, userToFollow);
            }
        }
    }

    async Task CreateUsersAsync(IReadOnlyList<CreateUserCmd> users)
    {
        foreach (var cmd in users) await userRepository.CreateUserAsync(cmd).ConfigureAwait(false);
    }


    static IReadOnlyList<CreateUserCmd> UserCommands() =>
    [
        new("johndoe", "John Doe"),
        new("janedoe", "Jane Doe"),
        new("bobsmith", "Bob Smith"),
        new("alicesmith", "Alice Smith"),
        new("johnnyappleseed", "Johnny Appleseed"),
        new("maryjane", "Mary Jane"),
        new("jasonjones", "Jason Jones"),
        new("sarahbrown", "Sarah Brown"),
        new("jamesbond", "James Bond"),
        new("janeausten", "Jane Austen"),
        new("johnwayne", "John Wayne"),
        new("marilynmonroe", "Marilyn Monroe"),
        new("elvispresley", "Elvis Presley"),
        new("albert Einstein", "Albert Einstein"),
        new("charlesdarwin", "Charles Darwin"),
        new("leonardodavinci", "Leonardo da Vinci"),
        new("emilybronte", "Emily Bronte"),
        new("hermanmelville", "Herman Melville"),
        new("johngalileo", "John Galileo"),
        new("juliuscaesar", "Julius Caesar"),
        new("napoleonbonaparte", "Napoleon Bonaparte")
    ];
}