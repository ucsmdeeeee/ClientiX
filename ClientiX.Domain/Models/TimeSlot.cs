using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Domain.Models
{
    public class TimeSlot
    {
        public string Time { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}
