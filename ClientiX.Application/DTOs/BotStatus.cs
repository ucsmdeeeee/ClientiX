using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Application.DTOs
{
    public class BotStatus
    {
        public bool IsSubscribed { get; set; }
        public bool IsBotRunning { get; set; }
        public string ExpiresAt { get; set; } = string.Empty;
    }
}
