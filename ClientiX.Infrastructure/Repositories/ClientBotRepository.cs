using ClientiX.Application.Interfaces;
using ClientiX.Domain.Models;
using ClientiX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ClientiX.Infrastructure.Repositories
{
    public class ClientBotRepository : IClientBotRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientBotRepository(ApplicationDbContext context) => _context = context;

        public async Task<ClientBot?> GetByBotIdAsync(long botId) =>
            await _context.ClientBots.FindAsync(botId);

        public async Task<ClientBot?> GetByTokenAsync(string token) =>
            await _context.ClientBots.FirstOrDefaultAsync(b => b.TelegramBotToken == token);

        public async Task<ClientBot> CreateOrUpdateAsync(ClientBot bot)
        {
            var existing = await _context.ClientBots.FindAsync(bot.TelegramBotId);
            if (existing == null)
                _context.ClientBots.Add(bot);
            else
                _context.ClientBots.Update(bot);
            await _context.SaveChangesAsync();
            return bot;
        }

        public async Task SetRunningStatusAsync(long botId, bool isRunning)
        {
            var bot = await _context.ClientBots.FindAsync(botId);
            if (bot != null)
            {
                bot.IsRunning = isRunning;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ClientBot>> GetActiveBotsAsync() =>
            await _context.ClientBots.Where(b => b.IsRunning).ToListAsync();
    }
}
