namespace Connextion.Web.Components;

public class QuickPostViewModel(ILogger<QuickPostViewModel> logger)
{
    readonly ILogger<QuickPostViewModel> _logger = logger;

    public string StatusText { get; set; } = "";
    
    public async Task SubmitAsync()
    {
        Connextion.Lib.PostsModule.addPost(StatusText);
        
        _logger.LogInformation("Post: {StatusText}", StatusText);
        await Task.Delay(2).ConfigureAwait(false);
        StatusText = "";
    }
}