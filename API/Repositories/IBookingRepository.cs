using API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Booking booking);
    Task<List<Booking>> GetBookingsByUserIdAsync(int userId);
    Task<Booking?> GetBookingByIdAsync(int bookingId);
    Task<bool> CancelBookingAsync(int bookingId);
}
