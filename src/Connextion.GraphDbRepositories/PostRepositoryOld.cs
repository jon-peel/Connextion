using Connextion.OldD;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Connextion.GraphDbRepositories;

public static class Mapping
{
    public static ProfileSummary MiniProfile(IReadOnlyDictionary<string, object> userData)
    {
        var username = userData["username"].As<string>();
        var displayName = userData["displayName"].As<string>();
        var degrees = userData["degrees"].As<byte>();
        return new (username, displayName, degrees);
    }

    public static PostOld Post(IReadOnlyDictionary<string, object> postData)
    {
        var id = Guid.Parse(postData["id"].As<string>());
        var user = MiniProfile(postData["postedBy"].As<IReadOnlyDictionary<string, object>>());
        var postedAt = postData["postedAt"].As<DateTime>();
        var body = postData["body"].As<string>();
        return new (id, user, postedAt, body);
    }

    public static OldD.Profile Profile(IReadOnlyDictionary<string, object> data)
    {
        var user = MiniProfile(data["user"].As<IReadOnlyDictionary<string, object>>());
        var posts = data["posts"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(Post)
            .ToArray();
        var following = data["following"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(MiniProfile)
            .ToArray();
        var followers = data["followers"]
            .As<IEnumerable<IReadOnlyDictionary<string, object>>>()
            .Select(MiniProfile)
            .ToArray();
        return new OldD.Profile(user, posts, following, followers);
    }
}