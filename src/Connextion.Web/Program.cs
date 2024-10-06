using Connextion.Web.Components;
using Connextion.Graph;
using Connextion.Posts;
using Connextion.ViewModels;
using Connextion.Web.Components.Posts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication
    .CreateBuilder(args);
//.UseUrls("http://*:80");


// Add services to the container.
builder.Services
    .AddGraphDb()
    .AddViewModels()
    .AddTransient<QuickPostViewModelFactory>(sx =>
        user => new QuickPostViewModel(sx.GetRequiredService<IPostRepository>(), user))
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();
app.Urls.Add("http://*:8080");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.ConfigureGraphDb().Run();