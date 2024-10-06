using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

internal class ConfigureTheDatabase(IDriver driver, IUserRepository userRepository)
{
    public async Task RunAsync()
    {
        await ConfigureConstraintsAsync().ConfigureAwait(false);
        var users = await userRepository.GetUsernamesAsync().ConfigureAwait(false);
        if (users.Any()) return;
        await ConfigureDefaultUsers().ConfigureAwait(false);
    }

    private async Task ConfigureConstraintsAsync()
    {
        string[] constraints =
        [
            "CREATE CONSTRAINT IF NOT EXISTS FOR (user:User) REQUIRE user.userName IS UNIQUE",
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

    private async Task ConfigureDefaultUsers()
    {
        var (_, results) = await driver.ExecutableQuery(
                """
                CREATE 
                // Create 20 User nodes
                (u1:User {userName: "jdoe", fullName: "John Doe"}),
                (u2:User {userName: "jsmith", fullName: "Jane Smith"}),
                (u3:User {userName: "mjohnson", fullName: "Michael Johnson"}),
                (u4:User {userName: "edavis", fullName: "Emily Davis"}),
                (u5:User {userName: "jbrown", fullName: "James Brown"}),
                (u6:User {userName: "ptaylor", fullName: "Patricia Taylor"}),
                (u7:User {userName: "rmiller", fullName: "Robert Miller"}),
                (u8:User {userName: "landerson", fullName: "Linda Anderson"}),
                (u9:User {userName: "dwilson", fullName: "David Wilson"}),
                (u10:User {userName: "bthomas", fullName: "Barbara Thomas"}),
                (u11:User {userName: "jjackson", fullName: "Joseph Jackson"}),
                (u12:User {userName: "lwhite", fullName: "Lisa White"}),
                (u13:User {userName: "charris", fullName: "Charles Harris"}),
                (u14:User {userName: "kmartin", fullName: "Karen Martin"}),
                (u15:User {userName: "dthompson", fullName: "Daniel Thompson"}),
                (u16:User {userName: "sgarcia", fullName: "Susan Garcia"}),
                (u17:User {userName: "pmartinez", fullName: "Paul Martinez"}),
                (u18:User {userName: "srobinson", fullName: "Sarah Robinson"}),
                (u19:User {userName: "mclark", fullName: "Mark Clark"}),
                (u20:User {userName: "nlewis", fullName: "Nancy Lewis"}),

                // Create hardcoded FOLLOWS relationships
                (u1)-[:FOLLOWS]->(u2),  // John Doe follows Jane Smith
                (u1)-[:FOLLOWS]->(u3),  // John Doe follows Michael Johnson
                (u2)-[:FOLLOWS]->(u4),  // Jane Smith follows Emily Davis
                (u3)-[:FOLLOWS]->(u5),  // Michael Johnson follows James Brown
                (u4)-[:FOLLOWS]->(u6),  // Emily Davis follows Patricia Taylor
                (u5)-[:FOLLOWS]->(u7),  // James Brown follows Robert Miller
                (u6)-[:FOLLOWS]->(u8),  // Patricia Taylor follows Linda Anderson
                (u7)-[:FOLLOWS]->(u1),  // Robert Miller follows John Doe
                (u8)-[:FOLLOWS]->(u9),  // Linda Anderson follows David Wilson
                (u9)-[:FOLLOWS]->(u10), // David Wilson follows Barbara Thomas
                (u10)-[:FOLLOWS]->(u11), // Barbara Thomas follows Joseph Jackson
                (u11)-[:FOLLOWS]->(u12), // Joseph Jackson follows Lisa White
                (u12)-[:FOLLOWS]->(u13), // Lisa White follows Charles Harris
                (u13)-[:FOLLOWS]->(u14), // Charles Harris follows Karen Martin
                (u14)-[:FOLLOWS]->(u15), // Karen Martin follows Daniel Thompson
                (u15)-[:FOLLOWS]->(u16), // Daniel Thompson follows Susan Garcia
                (u16)-[:FOLLOWS]->(u17), // Susan Garcia follows Paul Martinez
                (u17)-[:FOLLOWS]->(u18), // Paul Martinez follows Sarah Robinson
                (u18)-[:FOLLOWS]->(u19), // Sarah Robinson follows Mark Clark
                (u19)-[:FOLLOWS]->(u20), // Mark Clark follows Nancy Lewis
                (u20)-[:FOLLOWS]->(u1),  // Nancy Lewis follows John Doe

                // Some reciprocal follow-backs
                (u2)-[:FOLLOWS]->(u1),  // Jane Smith follows back John Doe
                (u4)-[:FOLLOWS]->(u2),  // Emily Davis follows back Jane Smith
                (u7)-[:FOLLOWS]->(u6),  // Robert Miller follows back Patricia Taylor
                (u13)-[:FOLLOWS]->(u12), // Charles Harris follows back Lisa White
                (u19)-[:FOLLOWS]->(u18); // Mark Clark follows back Sarah Robinson
                """
            )
            .ExecuteAsync()
            .ConfigureAwait(false);
        Console.WriteLine(results);
    }
    
}