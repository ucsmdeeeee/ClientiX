using ClientiX.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);
        Task<IEnumerable<Booking>> GetUpcomingByMasterIdAsync(long masterUserId);
        Task<Booking> CreateAsync(Booking booking);
        Task UpdateStatusAsync(Guid bookingId, Domain.Enums.BookingStatus status);
    }
}
