using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

internal class ConfigureTheDatabase(
    IDriver driver,
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IPostRepository postRepository)
{
    public async Task RunAsync()
    {
        await ConfigureConstraintsAsync().ConfigureAwait(false);
        var hasUsers = await userRepository.GetAllUsersAsync().AnyAsync().ConfigureAwait(false);
        if (!hasUsers) await ConfigureDefaultUsers().ConfigureAwait(false);
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
            var numPosts = random.Next(1, 4);
            for (var i = 0; i < numPosts; i++)
            {
                var postCmd = new PostCreated(
                    new(Guid.NewGuid()),
                    new(user.Username),
                    CreateRandomDate(),
                    new($"Post {i+1} from {user.DisplayName}"));
                await postRepository.CreatePostAsync(postCmd).ConfigureAwait(false);
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
                var cmd = new FollowCmd(user.Username, userToFollow);
                await profileRepository.FollowAsync(cmd);
            }
        }
    }

    async Task CreateUsersAsync(IReadOnlyList<CreateUserCmd> users)
    {
        foreach (var cmd in users) await userRepository.CreateUserAsync(cmd).ConfigureAwait(false);
    }


    static IReadOnlyList<CreateUserCmd> UserCommands() =>
    [
        new("johndoe", "John Doe", "Just a regular guy"),
        new("janedoe", "Jane Doe", "Just a regular gal"),
        new("bobsmith", "Bob Smith", "A man of mystery"),
        new("alicesmith", "Alice Smith", "A woman of wonder"),
        new("johnnyappleseed", "Johnny Appleseed", "The legendary planter"),
        new("maryjane", "Mary Jane", "A free spirit"),
        new("jasonjones", "Jason Jones", "A tech whiz"),
        new("sarahbrown", "Sarah Brown", "A social butterfly"),
        new("jamesbond", "James Bond", "The spy who loved me"),
        new("janeausten", "Jane Austen", "The author of Pride and Prejudice"),
        new("johnwayne", "John Wayne", "The Duke"),
        new("marilynmonroe", "Marilyn Monroe", "A Hollywood legend"),
        new("elvispresley", "Elvis Presley", "The King of Rock 'n' Roll"),
        new("alberteinstein", "Albert Einstein", "The genius behind relativity"),
        new("charlesdarwin", "Charles Darwin", "The father of evolution"),
        new("leonardodavinci", "Leonardo da Vinci", "The Renaissance man"),
        new("emilybronte", "Emily Bronte", "The author of Wuthering Heights"),
        new("hermanmelville", "Herman Melville", "The author of Moby-Dick"),
        new("johngalileo", "John Galileo", "The father of modern physics"),
        new("juliuscaesar", "Julius Caesar", "The Roman general"),
        new("napoleonbonaparte", "Napoleon Bonaparte", "The French general")
    ];
}