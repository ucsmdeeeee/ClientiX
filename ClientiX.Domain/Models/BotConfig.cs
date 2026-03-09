using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Domain.Models
{
    public class BotConfig
    {
        public string WelcomeText { get; set; } = "Добро пожаловать! 👋";
        public List<Service> Services { get; set; } = new();
        public List<string> PortfolioLinks { get; set; } = new();
        public Dictionary<string, List<TimeSlot>> Schedule { get; set; } = new();
        public string? ChannelUsername { get; set; }
        public bool RequireChannelSubscription { get; set; }
    }
}
