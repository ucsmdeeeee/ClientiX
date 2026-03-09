using ClientiX.Application.Interfaces;
using ClientiX.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Application.DependencyInjection
{
    public static class ApplicationServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Application Services
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<INotificationService, NotificationService>();

            // Hosted Services
            services.AddHostedService<MainBotHostedService>();
            services.AddHostedService<ClientBotsHostedService>();
            services.AddHostedService<SubscriptionCheckerService>();
            services.AddHostedService<BookingNotifierService>();

            return services;
        }
    }
}
