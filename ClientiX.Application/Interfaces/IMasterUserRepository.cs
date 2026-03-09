using ClientiX.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Application.Interfaces
{
    public interface IMasterUserRepository
    {
        Task<MasterUser?> GetByTelegramIdAsync(long telegramUserId);
        Task<MasterUser?> GetByClientBotIdAsync(long clientBotId);
        Task<IEnumerable<MasterUser>> GetAllAsync();
        Task<MasterUser> CreateOrUpdateAsync(MasterUser user);
        Task<bool> BlockUserAsync(long telegramUserId);
    }
}
