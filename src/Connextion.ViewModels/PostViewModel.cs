namespace Connextion.ViewModels;

public class PostViewModel(TimeLinePostDto post)
{
    public string Body { get; } = post.Body;
    public string PostedBy { get; } = post.PostedBy.DisplayName;
    public string Time { get; } = CreateTime(post);
    public string AuthorProfileUrl { get; } =  $"/profile/{post.PostedBy.Username}";
    
    static string CreateTime(TimeLinePostDto post) => post.PostedAt switch
    {
        var t when t > DateTime.Today => t.ToString("HH:mm"),
        var y when y > DateTime.Today.AddDays(-1) => $"Yesterday, {y:HH:mm}",
        var w when w > DateTime.Today.AddDays(-7) => $"{Enum.GetName(w.DayOfWeek)}, {w:HH:mm}",
        _ => post.PostedAt.ToString("d MMMM yyyy, HH:mm")
    };
}