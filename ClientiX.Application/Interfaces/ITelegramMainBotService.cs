using ClientiX.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ClientiX.Application.Interfaces
{
    public interface ITelegramMainBotService
    {
        Task HandleUpdateAsync(Update update);
    }

}
