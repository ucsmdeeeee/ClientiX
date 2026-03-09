using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Domain.Models
{
    public class ClientBot
    {
        public long TelegramBotId { get; set; }
        public string TelegramBotToken { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
        public string ConfigJson { get; set; } = string.Empty;

        public MasterUser? MasterUser { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }
}
