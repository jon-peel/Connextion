using Connextion.ViewModels.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace Connextion.ViewModels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        return services
            .AddTransient<TimeLineViewModel>()
            .AddTransient<UserProfileViewModel>()
            .AddTransient<QuickPostViewModel>();
    }
}