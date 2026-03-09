using ClientiX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.ConsoleApp.Services
{
    public class SubscriptionCheckerService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SubscriptionCheckerService> _logger;

        public SubscriptionCheckerService(IServiceProvider services, ILogger<SubscriptionCheckerService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var expired = await context.Subscriptions
                    .Where(s => s.IsActive && s.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var sub in expired)
                {
                    sub.IsActive = false;
                    _logger.LogInformation("Subscription {Id} expired", sub.Id);
                }

                await context.SaveChangesAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            }
        }
    }
}
