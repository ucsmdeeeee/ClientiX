using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ClientiX.Application.Interfaces
{
    public interface ITelegramClientBotService
    {
        Task HandleUpdateAsync(Update update);
    }
}
