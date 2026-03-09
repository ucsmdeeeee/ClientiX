using ClientiX.Application.Interfaces;
using ClientiX.Application.Services;
using ClientiX.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace ClientiX.Infrastructure.Configurations
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddTelegramServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<ITelegramBotClient>(sp =>
                new TelegramBotClient(configuration["MainBotToken"]!));

            services.AddScoped<ITelegramMainBotService, TelegramMainBotService>();
            services.AddScoped<IPdfReportService, PdfReportService>();
            services.AddScoped<SubscriptionService>();

            return services;
        }
    }
}
