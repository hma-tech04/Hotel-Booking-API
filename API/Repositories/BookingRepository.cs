using API.Data;
using API.DTOs.Statistics;
using API.Enum;
using API.Models;
using Microsoft.EntityFrameworkCore;
public class BookingRepository : IBookingRepository
{
    private readonly HotelBookingContext _context;

    public BookingRepository(HotelBookingContext context)
    {
        _context = context;
    }

    public async Task<Booking> AddBookingAsync(Booking booking)
    {
        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<List<Booking>> GetBookingsByUserIdAsync(int userId)
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
    }

    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null) return false;

        booking.BookingStatus = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateBookingAsync(Booking booking)
    {
        var existingBooking = await _context.Bookings.FindAsync(booking.BookingId);
        if (existingBooking == null) return false;

        existingBooking.CheckInDate = booking.CheckInDate;
        existingBooking.CheckOutDate = booking.CheckOutDate;
        existingBooking.TotalPrice = booking.TotalPrice;
        existingBooking.BookingStatus = booking.BookingStatus;

        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null) return false;

        booking.BookingStatus = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Booking>> GetAllBooking()
    {
        return await _context.Bookings.ToListAsync();
    }

    public async Task<List<Booking>> RetrieveBookingStats(DateTime start, DateTime end)
    {
        return await _context.Bookings
            .Where(b => b.CheckOutDate >= start && b.CheckInDate <= end)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetUncheckedInBookingsByPhoneNumber(PhoneNumberRequestDto requestDto)
    {
        return await _context.Bookings
        .Include(b => b.User)
        .Where(b => b.ActualCheckInTime == null 
            && b.User.PhoneNumber == requestDto.PhoneNumber 
            && b.BookingStatus != BookingStatus.Cancelled)
        .ToListAsync();
    }

    public async Task<List<Booking>> GetUncheckOutBookingsByPhoneNumber(PhoneNumberRequestDto requestDto)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Where(b => b.ActualCheckOutTime == null 
                    && b.ActualCheckInTime != null
                    && b.User.PhoneNumber == requestDto.PhoneNumber
                    && b.BookingStatus != BookingStatus.Cancelled)
            .ToListAsync();

    }
}
