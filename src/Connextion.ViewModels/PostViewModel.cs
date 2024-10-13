namespace Connextion.ViewModels;

public class PostViewModel(TimeLinePostDto post)
{
    public PostViewModel(Post post, User currentUser) 
        : this(new(post.Id.Value, currentUser, post.PostedAt, post.Body.Value)) {}
    
    public string Body { get; } = post.Body;
    public string PostedBy { get; } = post.PostedBy.DisplayName.Value;
    public string Time { get; } = CreateTime(post);
    public string AuthorProfileUrl { get; } =  $"/profile/{post.PostedBy.Id.Value}";
    
    static string CreateTime(TimeLinePostDto post) => post.PostedAt switch
    {
        var t when t > DateTime.Today => t.ToString("HH:mm"),
        var y when y > DateTime.Today.AddDays(-1) => $"Yesterday, {y:HH:mm}",
        var w when w > DateTime.Today.AddDays(-7) => $"{Enum.GetName(w.DayOfWeek)}, {w:HH:mm}",
        _ => post.PostedAt.ToString("d MMMM yyyy, HH:mm")
    };
}