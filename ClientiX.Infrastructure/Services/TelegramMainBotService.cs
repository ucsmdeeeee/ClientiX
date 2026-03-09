using ClientiX.Application.Interfaces;
using ClientiX.Application.Services;
using ClientiX.Domain.Models;
using ClientiX.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ClientiX.Infrastructure.Services;

public class TelegramMainBotService : ITelegramMainBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMasterUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IClientBotRepository _botRepository;
    private readonly SubscriptionService _subscriptionService; // ✅ ДОБАВИЛИ
    private readonly ILogger<TelegramMainBotService> _logger;

    public TelegramMainBotService(
        ITelegramBotClient botClient,
        IMasterUserRepository userRepository,
        ISubscriptionRepository subscriptionRepository, // ✅ ISubscriptionRepository
        IClientBotRepository botRepository,
        ILogger<TelegramMainBotService> logger)
    {
        _botClient = botClient;
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _botRepository = botRepository;
        _subscriptionService = new SubscriptionService(subscriptionRepository, userRepository); // ✅
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update)
    {
        if (update.Type != UpdateType.Message) return;

        var message = update.Message!;
        var userId = message.From!.Id;
        var text = message.Text ?? "";

        _logger.LogInformation("MainBot: {UserId} sent: {Text}", userId, text);

        var user = await _userRepository.GetByTelegramIdAsync(userId)
            ?? new Domain.Models.MasterUser { TelegramUserId = userId };

        await HandleCommandAsync(user, text, message.Chat.Id);
    }

    private async Task HandleCommandAsync(Domain.Models.MasterUser user, string text, long chatId)
    {
        if (text == "/start")
        {
            await ShowTariffs(chatId);
        }
        else if (text.StartsWith("/token"))
        {
            await HandleTokenCommand(user, text, chatId);
        }
        else if (text == "/config")
        {
            await ShowConfigMenu(chatId);
        }
        else if (text == "/start_bot")
        {
            await StartClientBot(user, chatId);
        }
        else if (text == "/stop_bot")
        {
            await StopClientBot(user, chatId);
        }
        else if (text == "/status")
        {
            await ShowStatus(user, chatId);
        }
        else if (text.StartsWith("/admin"))
        {
            await HandleAdminCommand(user, text, chatId);
        }
        else
        {
            await _botClient.SendTextMessageAsync(chatId, "❓ Используйте /start для начала работы");
        }
    }


    private async Task ShowTariffs(long chatId)
    {
        var tariffs = _subscriptionService.GetTariffs(); // ✅ Service!

        var keyboard = new InlineKeyboardMarkup(new[] // ✅ new[]
        {
            InlineKeyboardButton.WithCallbackData("💎 500₽ / 30 дней", "buy_Monthly500"),
            InlineKeyboardButton.WithCallbackData("💎 1200₽ / 90 дней", "buy_Quarterly1200"),
            InlineKeyboardButton.WithCallbackData("💎 2000₽ / 180 дней", "buy_HalfYear2000"),
            InlineKeyboardButton.WithCallbackData("🎁 Пробная 7 дней", "buy_Trial7Days")
        });

        await _botClient.SendTextMessageAsync(chatId,
            "💎 <b>ТАРИФЫ SaaS бота:</b>\n\n" + // ✅ \n
            "• <b>500₽</b> / 30 дней\n" +
            "• <b>1200₽</b> / 90 дней (экономия 20%)\n" +
            "• <b>2000₽</b> / 180 дней (экономия 33%)\n\n" +
            "🎁 <i>Пробная 7 дней БЕСПЛАТНО!</i>",
            replyMarkup: keyboard, parseMode: ParseMode.Html); // ✅ parseMode lowercase
    }

    private async Task HandleTokenCommand(Domain.Models.MasterUser user, string text, long chatId)
    {
        var token = text.Replace("/token ", "").Trim();
        if (string.IsNullOrEmpty(token))
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Укажите токен: /token ABC123...");
            return;
        }

        var clientBot = new Domain.Models.ClientBot
        {
            TelegramBotToken = token,
            TelegramBotId = 0 // Обновится позже
        };

        await _botRepository.CreateOrUpdateAsync(clientBot);
        user.ClientBotId = clientBot.TelegramBotId;
        await _userRepository.CreateOrUpdateAsync(user);

        await _botClient.SendTextMessageAsync(chatId, $"✅ Токен сохранен: {token[..10]}...");
    }

    // Остальные методы без изменений...
    private async Task ShowStatus(Domain.Models.MasterUser user, long chatId)
    {
        var sub = await _subscriptionRepository.GetActiveByUserIdAsync(user.TelegramUserId);
        var status = $"🆔 ID: {user.TelegramUserId}\n" +
                    $"📱 Бот: {user.ClientBotId ?? 0}\n" +
                    $"✅ Подписка: {(sub?.IsActive == true ? "Активна" : "Нет")}\n" +
                    (sub != null ? $"📅 До: {sub.ExpiresAt:dd.MM.yyyy}" : "");

        await _botClient.SendTextMessageAsync(chatId, status);
    }

    private async Task StartClientBot(Domain.Models.MasterUser user, long chatId)
    {
        await _botClient.SendTextMessageAsync(chatId,
            user.ClientBotId.HasValue ? "✅ Клиентский бот запущен" : "❌ Сначала /token");
    }

    private async Task StopClientBot(Domain.Models.MasterUser user, long chatId) =>
        await _botClient.SendTextMessageAsync(chatId, "⏹️ Клиентский бот остановлен");

    private async Task HandleAdminCommand(Domain.Models.MasterUser user, string text, long chatId)
    {
        if (!user.IsAdmin)
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Нет доступа");
            return;
        }

        if (text.StartsWith("/admin_trial "))
        {
            var targetId = long.Parse(text.Replace("/admin_trial ", ""));
            await _botClient.SendTextMessageAsync(chatId, $"✅ Пробная выдана {targetId}");
        }
        else if (text.StartsWith("/admin_block "))
        {
            var targetId = long.Parse(text.Replace("/admin_block ", ""));
            await _userRepository.BlockUserAsync(targetId);
            await _botClient.SendTextMessageAsync(chatId, $"✅ {targetId} заблокирован");
        }
        else
        {
            await _botClient.SendTextMessageAsync(chatId, "🔧 <b>Админ команды:</b>\n/admin_trial 123456\n/admin_block 123456");
        }
    }

    private async Task ShowConfigMenu(long chatId) =>
        await _botClient.SendTextMessageAsync(chatId, "⚙️ Конфигурация (заглушка)");
}
