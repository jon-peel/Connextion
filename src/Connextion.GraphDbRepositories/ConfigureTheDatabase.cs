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
        var users = await userRepository.GetUsernamesAsync().ConfigureAwait(false);
        if (users.Length != 0) return;
        await ConfigureDefaultUsers().ConfigureAwait(false);
    }

    async Task ConfigureConstraintsAsync()
    {
        string[] constraints =
        [
            "CREATE CONSTRAINT IF NOT EXISTS FOR (user:User) REQUIRE user.username IS UNIQUE",
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

    private async Task CreatePostsAsync(IReadOnlyList<CreateUserCmd> users)
    {
        var random = new Random();
        foreach (var user in users)
        {
            var numPosts = random.Next(1, 23);
            for (var i = 0; i < numPosts; i++)
            {
                var postCmd = new CreatePostCmd(
                    Username: user.Username,
                    Status: $"Status {i} from {user.FullName}",
                    PostedAt: CreateRandomDate()
                );
                await postRepository.SubmitStatusAsync(postCmd).ConfigureAwait(false);
            }
        }
        return;
        
        DateTime CreateRandomDate()
        {
            var month = random.Next(1, 9);
            var day = random.Next(1, 28);
            return new (2024, month, day);
        }
    }

    private async Task FollowUsersAsync(IReadOnlyList<CreateUserCmd> users)
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
                await profileRepository.FollowAsync(user.Username, userToFollow);
            }
        }
    }

    private async Task CreateUsersAsync(IReadOnlyList<CreateUserCmd> users)
    {
        foreach (var cmd in  users) await userRepository.CreateUserAsync(cmd).ConfigureAwait(false);
    }


    static IReadOnlyList<CreateUserCmd> UserCommands() =>
        new[]
        {
            new CreateUserCmd("johndoe", "John Doe"),
            new CreateUserCmd("janedoe", "Jane Doe"),
            new CreateUserCmd("bobsmith", "Bob Smith"),
            new CreateUserCmd("alicesmith", "Alice Smith"),
            new CreateUserCmd("johnnyappleseed", "Johnny Appleseed"),
            new CreateUserCmd("maryjane", "Mary Jane"),
            new CreateUserCmd("jasonjones", "Jason Jones"),
            new CreateUserCmd("sarahbrown", "Sarah Brown"),
            new CreateUserCmd("jamesbond", "James Bond"),
            new CreateUserCmd("janeausten", "Jane Austen"),
            new CreateUserCmd("johnwayne", "John Wayne"),
            new CreateUserCmd("marilynmonroe", "Marilyn Monroe"),
            new CreateUserCmd("elvispresley", "Elvis Presley"),
            new CreateUserCmd("albert Einstein", "Albert Einstein"),
            new CreateUserCmd("charlesdarwin", "Charles Darwin"),
            new CreateUserCmd("leonardodavinci", "Leonardo da Vinci"),
            new CreateUserCmd("emilybronte", "Emily Bronte"),
            new CreateUserCmd("hermanmelville", "Herman Melville"),
            new CreateUserCmd("johngalileo", "John Galileo"),
            new CreateUserCmd("juliuscaesar", "Julius Caesar"),
            new CreateUserCmd("napoleonbonaparte", "Napoleon Bonaparte"),
        };

//         
//        
//         
//         
//         var (_, results) = await driver.ExecutableQuery(
//                 """
//                 CREATE 
//                 // Create 20 User nodes
//                 (u1:User {username: "jdoe", fullName: "John Doe"}),
//                 (u2:User {username: "jsmith", fullName: "Jane Smith"}),
//                 (u3:User {username: "mjohnson", fullName: "Michael Johnson"}),
//                 (u4:User {username: "edavis", fullName: "Emily Davis"}),
//                 (u5:User {username: "jbrown", fullName: "James Brown"}),
//                 (u6:User {username: "ptaylor", fullName: "Patricia Taylor"}),
//                 (u7:User {username: "rmiller", fullName: "Robert Miller"}),
//                 (u8:User {username: "landerson", fullName: "Linda Anderson"}),
//                 (u9:User {username: "dwilson", fullName: "David Wilson"}),
//                 (u10:User {username: "bthomas", fullName: "Barbara Thomas"}),
//                 (u11:User {username: "jjackson", fullName: "Joseph Jackson"}),
//                 (u12:User {username: "lwhite", fullName: "Lisa White"}),
//                 (u13:User {username: "charris", fullName: "Charles Harris"}),
//                 (u14:User {username: "kmartin", fullName: "Karen Martin"}),
//                 (u15:User {username: "dthompson", fullName: "Daniel Thompson"}),
//                 (u16:User {username: "sgarcia", fullName: "Susan Garcia"}),
//                 (u17:User {username: "pmartinez", fullName: "Paul Martinez"}),
//                 (u18:User {username: "srobinson", fullName: "Sarah Robinson"}),
//                 (u19:User {username: "mclark", fullName: "Mark Clark"}),
//                 (u20:User {username: "nlewis", fullName: "Nancy Lewis"}),
//
//                 // Create hardcoded FOLLOWS relationships
//                 (u1)-[:FOLLOWS]->(u2),  // John Doe follows Jane Smith
//                 (u1)-[:FOLLOWS]->(u3),  // John Doe follows Michael Johnson
//                 (u2)-[:FOLLOWS]->(u4),  // Jane Smith follows Emily Davis
//                 (u3)-[:FOLLOWS]->(u5),  // Michael Johnson follows James Brown
//                 (u4)-[:FOLLOWS]->(u6),  // Emily Davis follows Patricia Taylor
//                 (u5)-[:FOLLOWS]->(u7),  // James Brown follows Robert Miller
//                 (u6)-[:FOLLOWS]->(u8),  // Patricia Taylor follows Linda Anderson
//                 (u7)-[:FOLLOWS]->(u1),  // Robert Miller follows John Doe
//                 (u8)-[:FOLLOWS]->(u9),  // Linda Anderson follows David Wilson
//                 (u9)-[:FOLLOWS]->(u10), // David Wilson follows Barbara Thomas
//                 (u10)-[:FOLLOWS]->(u11), // Barbara Thomas follows Joseph Jackson
//                 (u11)-[:FOLLOWS]->(u12), // Joseph Jackson follows Lisa White
//                 (u12)-[:FOLLOWS]->(u13), // Lisa White follows Charles Harris
//                 (u13)-[:FOLLOWS]->(u14), // Charles Harris follows Karen Martin
//                 (u14)-[:FOLLOWS]->(u15), // Karen Martin follows Daniel Thompson
//                 (u15)-[:FOLLOWS]->(u16), // Daniel Thompson follows Susan Garcia
//                 (u16)-[:FOLLOWS]->(u17), // Susan Garcia follows Paul Martinez
//                 (u17)-[:FOLLOWS]->(u18), // Paul Martinez follows Sarah Robinson
//                 (u18)-[:FOLLOWS]->(u19), // Sarah Robinson follows Mark Clark
//                 (u19)-[:FOLLOWS]->(u20), // Mark Clark follows Nancy Lewis
//                 (u20)-[:FOLLOWS]->(u1),  // Nancy Lewis follows John Doe
//
//                 // Some reciprocal follow-backs
//                 (u2)-[:FOLLOWS]->(u1),  // Jane Smith follows back John Doe
//                 (u4)-[:FOLLOWS]->(u2),  // Emily Davis follows back Jane Smith
//                 (u7)-[:FOLLOWS]->(u6),  // Robert Miller follows back Patricia Taylor
//                 (u13)-[:FOLLOWS]->(u12), // Charles Harris follows back Lisa White
//                 (u19)-[:FOLLOWS]->(u18); // Mark Clark follows back Sarah Robinson
//                 """
//             )
//             .ExecuteAsync()
//             .ConfigureAwait(false);
//         Console.WriteLine(results);
//     }
}