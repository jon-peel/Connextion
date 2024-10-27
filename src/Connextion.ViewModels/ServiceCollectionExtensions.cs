using Connextion.ViewModels.Events;
using Connextion.ViewModels.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace Connextion.ViewModels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        return services
            .AddTransient<TimeLineViewModel>()
            .AddTransient<ProfileViewModel>()
            .AddTransient<MessagesViewModel>()
            .AddTransient<CreatePostViewModel>()
            .AddTransient<CreateEventViewModel>();
    }
}