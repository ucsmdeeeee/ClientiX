using ClientiX.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Domain.Models
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public SubscriptionPlan PlanType { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }

        public MasterUser? MasterUser { get; set; }
    }
}
