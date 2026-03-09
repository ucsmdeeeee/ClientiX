using ClientiX.Application.Interfaces;
using ClientiX.Domain.Models;
using ClientiX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Infrastructure.Repositories
{
    public class MasterUserRepository : IMasterUserRepository
    {
        private readonly ApplicationDbContext _context;

        public MasterUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MasterUser?> GetByTelegramIdAsync(long telegramUserId)
        {
            return await _context.MasterUsers
                .Include(u => u.Subscription)
                .Include(u => u.ClientBot)
                .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId);
        }

        public async Task<MasterUser?> GetByClientBotIdAsync(long clientBotId)
        {
            return await _context.MasterUsers
                .Include(u => u.Subscription)
                .Include(u => u.ClientBot)
                .FirstOrDefaultAsync(u => u.ClientBotId == clientBotId);
        }

        public async Task<IEnumerable<MasterUser>> GetAllAsync()
        {
            return await _context.MasterUsers
                .Include(u => u.Subscription)
                .Include(u => u.ClientBot)
                .ToListAsync();
        }

        public async Task<MasterUser> CreateOrUpdateAsync(MasterUser user)
        {
            var existing = await _context.MasterUsers.FindAsync(user.TelegramUserId);
            if (existing == null)
            {
                _context.MasterUsers.Add(user);
            }
            else
            {
                _context.MasterUsers.Update(user);
            }
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> BlockUserAsync(long telegramUserId)
        {
            var user = await _context.MasterUsers.FindAsync(telegramUserId);
            if (user != null)
            {
                user.SubscriptionId = null;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
