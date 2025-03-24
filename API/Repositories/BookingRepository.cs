using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        booking.BookingStatus = "Cancelled";
        await _context.SaveChangesAsync();
        return true;
    }
}
