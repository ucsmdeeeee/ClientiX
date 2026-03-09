using ClientiX.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Application.Interfaces
{
    public interface IClientBotRepository
    {
        Task<ClientBot?> GetByBotIdAsync(long botId);
        Task<ClientBot?> GetByTokenAsync(string token);
        Task<ClientBot> CreateOrUpdateAsync(ClientBot bot);
        Task SetRunningStatusAsync(long botId, bool isRunning);
        Task<IEnumerable<ClientBot>> GetActiveBotsAsync();
    }
}
