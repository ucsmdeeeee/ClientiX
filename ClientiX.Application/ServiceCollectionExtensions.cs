using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientiX.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClientiX.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<SubscriptionService>();
            return services;
        }
    }
}
