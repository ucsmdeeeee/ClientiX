using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Application.DTOs
{
    public class TariffInfo
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Days { get; set; }
    }
}
