using ClientiX.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClientiX.Application.Services
{
    public class BotConfigService
    {
        public static BotConfig ParseConfig(string configJson)
        {
            return JsonSerializer.Deserialize<BotConfig>(configJson) ?? new BotConfig();
        }

        public static string SerializeConfig(BotConfig config)
        {
            return JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public decimal CalculateServicePrice(BotConfig config, string serviceName, int durationMinutes)
        {
            var service = config.Services.FirstOrDefault(s => s.Name == serviceName);
            if (service != null)
            {
                var basePrice = service.Price;
                var additionalHour = (decimal)Math.Ceiling((durationMinutes - service.DurationMinutes) / 60.0);
                return basePrice + (additionalHour * 500);
            }
            return 0;
        }
    }
}
