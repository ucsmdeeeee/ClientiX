using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientiX.Domain.Enums;

namespace ClientiX.Domain.Models
{
    public class MasterUser
    {
        public long TelegramUserId { get; set; }
        public bool IsAdmin { get; set; }
        public Guid? SubscriptionId { get; set; }
        public long? ClientBotId { get; set; }

        public Subscription? Subscription { get; set; }
        public ClientBot? ClientBot { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }
}
