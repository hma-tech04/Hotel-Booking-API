using API.DTOs;
using API.Models;
using API.Repositories;
using AutoMapper;

public class BookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public BookingService(IBookingRepository bookingRepository, IMapper mapper, IRoomRepository roomRepository, IUserRepository userRepository)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<BookingDTO> AddBookingAsync(BookingDTO bookingDTO)
    {
        var user = await _userRepository.GetUseByIDAsync(bookingDTO.UserId);
        var room = await _roomRepository.GetRoomByIDAsync(bookingDTO.RoomId);
        if (user == null || room == null)
        {
            throw new CustomException(ErrorCode.NotFound, "User or Room not found");
        }
        var booking = _mapper.Map<Booking>(bookingDTO);
        booking.BookingStatus = API.Enum.BookingStatus.Confirmed;  

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
