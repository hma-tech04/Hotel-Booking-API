using API.Models;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllBookingAsync();
    Task<Booking?> GetBookingByIDAsync(int id);
    Task<Booking> AddBookingAsync(Booking booking);
    Task<Booking?> UpdateBookingAsync(Booking booking);
    Task<Booking?> DeleteBookingAsync(int id);
}