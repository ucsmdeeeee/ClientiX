using ClientiX.Application.Interfaces;
using ClientiX.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace ClientiX.ConsoleApp.Services
{
    public class BookingNotifierService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BookingNotifierService> _logger;

        public BookingNotifierService(IServiceProvider services, ILogger<BookingNotifierService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

                var upcoming = await bookingRepo.GetUpcomingByMasterIdAsync(0); // TODO: all masters

                foreach (var booking in upcoming.Where(b =>
                    b.Status == BookingStatus.Pending &&
                    b.BookingDateTime - DateTime.UtcNow <= TimeSpan.FromHours(12)))
                {
                    // Send notification
                    var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                        {
                            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("✅ Подтвердить", $"confirm_{booking.Id}"),
                            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("❌ Отменить", $"cancel_{booking.Id}")
                        }
                    );

                    await botClient.SendTextMessageAsync(
                        booking.ClientChatId,
                        $"⏰ Напоминание о записи {booking.BookingDateTime:dd.MM HH:mm}",
                        replyMarkup: keyboard);
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}
