using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Application.DTOs
{
    public class TelegramUpdateDto
    {
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public string? Text { get; set; }
        public string? CallbackData { get; set; }
        public bool IsCallbackQuery { get; set; }
    }
}
