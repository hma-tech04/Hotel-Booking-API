using API.Models;
public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Booking booking);
    Task<List<Booking>> GetBookingsByUserIdAsync(int userId);
    Task<Booking?> GetBookingByIdAsync(int bookingId);
    Task<bool> CancelBookingAsync(int bookingId);
}
