using ClientiX.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClientiX.ConsoleApp.Services;

public class ClientBotsHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ClientBotsHostedService> _logger;

    public ClientBotsHostedService(IServiceProvider services, ILogger<ClientBotsHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ClientBotsHostedService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateActiveClientBotsAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task UpdateActiveClientBotsAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var botRepository = scope.ServiceProvider.GetRequiredService<IClientBotRepository>();
        var activeBots = await botRepository.GetActiveBotsAsync();

        _logger.LogInformation("Found {Count} active client bots", activeBots.Count());

        foreach (var botData in activeBots) // ✅ botData объявлена здесь!
        {
            _logger.LogInformation("Client bot {BotId} is active", botData.TelegramBotId);
        }
    }
}
