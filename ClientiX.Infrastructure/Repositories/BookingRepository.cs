using ClientiX.Application.Interfaces;
using ClientiX.Domain.Enums;
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
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.MasterUser)
                .ThenInclude(u => u.ClientBot)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetUpcomingByMasterIdAsync(long masterUserId)
        {
            return await _context.Bookings
                .Where(b => b.MasterUserId == masterUserId
                         && b.BookingDateTime > DateTime.UtcNow
                         && b.Status != BookingStatus.Cancelled
                         && b.Status != BookingStatus.Completed)
                .OrderBy(b => b.BookingDateTime)
                .Take(20)
                .ToListAsync();
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task UpdateStatusAsync(Guid bookingId, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }
        }
    }
}
