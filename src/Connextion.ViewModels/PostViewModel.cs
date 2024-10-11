using Connextion.OldD;

namespace Connextion.ViewModels;

public class PostViewModel(PostOld postOld)
{
    public string Status { get; } = postOld.Status;
    public string PostedBy { get; } = postOld.PostedBy.DisplayName;

    public string Time { get; } = CreateTime(postOld);

    public string AuthorProfileUrl { get; } =  $"/profile/{postOld.PostedBy.Username}";
    
    static string CreateTime(PostOld postOld) => postOld.PostedAt switch
    {
        var t when t > DateTime.Today => t.ToString("HH:mm"),
        var y when y > DateTime.Today.AddDays(-1) => $"Yesterday, {y:HH:mm}",
        var w when w > DateTime.Today.AddDays(-7) => $"{Enum.GetName(w.DayOfWeek)}, {w:HH:mm}",
        _ => postOld.PostedAt.ToString("d MMMM yyyy, HH:mm")
    };
}