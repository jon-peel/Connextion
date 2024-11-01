using Connextion.ViewModels.Events;
using Connextion.ViewModels.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace Connextion.ViewModels;

// ViewModels are the C# classes that are used to expose data and functionality
// from the model to the view. They are used to bind the view to the model and
// to handle user input and other events. They are also responsible for
// providing data to the view in a format that is suitable for binding, and
// for handling commands and other user input.

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        return services
            .AddTransient<TimeLineViewModel>()
            .AddTransient<ProfileViewModel>()
            .AddTransient<MessagesViewModel>()
            .AddTransient<CreatePostViewModel>()
            .AddTransient<CreateEventViewModel>()
            .AddTransient<EventsViewModel>()
            .AddTransient<EventViewModel>()
            .AddTransient<SearchViewModel>();
    }
}