using API.DTOs.Statistics;
using API.Enum;
using API.Models;
public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Booking booking);
    Task<List<Booking>> GetBookingsByUserIdAsync(int userId);
    Task<Booking?> GetBookingByIdAsync(int bookingId);
    Task<bool> CancelBookingAsync(int bookingId);
    Task<bool> UpdateBookingAsync(Booking booking);
    Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status);
    Task<List<Booking>> GetAllBooking();
    Task<List<Booking>> RetrieveBookingStats(DateTime start, DateTime end);
    Task<List<Booking>> GetUncheckedInBookingsByPhoneNumber(PhoneNumberRequestDto requestDto);
}
