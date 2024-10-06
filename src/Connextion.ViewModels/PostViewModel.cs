namespace Connextion.ViewModels;

public class PostViewModel(Post post)
{
    public string Status { get; } = post.Status;
    public string PostedBy { get; } = post.PostedBy.FullName;
    
    public string Time { get; } = post.PostedAt switch
    {
        var t when t < DateTime.Today => t.ToString("HH:mm"),
        var y when y < DateTime.Today.AddDays(-1) => $"Yesterday, {y:HH:mm}",
        var w when w < DateTime.Today.AddDays(-7) => $"{Enum.GetName(w.DayOfWeek)}, {w:HH:mm}",
        _ => post.PostedAt.ToString("d MMMM yyyy, HH:mm")
    };

    public string AuthorProfileUrl { get; } =  $"/profile/{post.PostedBy.Username}";
}