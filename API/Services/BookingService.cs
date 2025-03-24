using API.DTOs;
using API.Models;
using API.Repositories;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public BookingService(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<BookingDTO> AddBookingAsync(BookingDTO bookingDTO)
    {
        var booking = _mapper.Map<Booking>(bookingDTO);
        booking.BookingStatus = "Confirmed";  // Mặc định là đã xác nhận

        var newBooking = await _bookingRepository.AddBookingAsync(booking);
        return _mapper.Map<BookingDTO>(newBooking);
    }

    public async Task<List<BookingDTO>> GetBookingsByUserIdAsync(int userId)
    {
        var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
        return _mapper.Map<List<BookingDTO>>(bookings);
    }

    public async Task<BookingDTO?> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        return booking == null ? null : _mapper.Map<BookingDTO>(booking);
    }

    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        return await _bookingRepository.CancelBookingAsync(bookingId);
    }
}
