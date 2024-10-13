using Connextion;
using Connextion.GraphDbRepositories;
using Connextion.Web.Components;
using Connextion.ViewModels;

var builder = WebApplication
    .CreateBuilder(args);
//.UseUrls("http://*:80");

// Add services to the container.
builder.Services
    .AddServices()
    .AddGraphDbRepositories()
    .AddViewModels()
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