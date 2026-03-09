using ClientiX.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Domain.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public long ClientChatId { get; set; }
        public long MasterUserId { get; set; }
        public DateTime BookingDateTime { get; set; }
        public BookingStatus Status { get; set; }

        public MasterUser? MasterUser { get; set; }
        public ClientBot? ClientBot { get; set; }
    }
}
