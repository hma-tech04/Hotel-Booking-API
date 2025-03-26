using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

public class BookingRepository : IBookingRepository
{
    private readonly HotelBookingContext _context;

    public BookingRepository(HotelBookingContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Booking>> GetAllBookingAsync()
    {
        return await _context.Bookings.ToListAsync();
    }

    public async Task<Booking?> GetBookingByIDAsync(int id)
    {
        return await _context.Bookings.FindAsync(id);
    }

    public async Task<Booking> AddBookingAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> UpdateBookingAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> DeleteBookingAsync(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return null;
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        return booking;
    }
}