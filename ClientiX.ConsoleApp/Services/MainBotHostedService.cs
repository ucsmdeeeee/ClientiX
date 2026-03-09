using ClientiX.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ClientiX.ConsoleApp.Services;

public class MainBotHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MainBotHostedService> _logger;

    public MainBotHostedService(IServiceProvider services, ILogger<MainBotHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        var mainBotService = scope.ServiceProvider.GetRequiredService<TelegramMainBotService>();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: async (client, update, ct) =>
            {
                try
                {
                    await mainBotService.HandleUpdateAsync(update);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling update");
                }
            },
            pollingErrorHandler: (client, exception, ct) =>
            {
                _logger.LogError(exception, "Polling error");
                return Task.CompletedTask;
            },
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("Main bot started polling");

        return Task.CompletedTask;
    }
}
