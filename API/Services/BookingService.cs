using API.DTOs;
using API.DTOs.EntityDTOs;
using API.DTOs.Request;
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

    public async Task<BookingDTO> AddBookingAsync(BookingRequest bookingRequest)
    {
        var user = await _userRepository.GetUserByIDAsync(bookingRequest.UserId);
        var room = await _roomRepository.GetRoomByIDAsync(bookingRequest.RoomId);
        if (user == null || room == null)
        {
            throw new CustomException(ErrorCode.NotFound, "User or Room not found");
        }
        if (room.IsAvailable != true)
        {
            throw new CustomException(ErrorCode.BadRequest, "Room is not available");
        }
        if (bookingRequest.CheckInDate < DateTime.Now || bookingRequest.CheckOutDate <= bookingRequest.CheckInDate)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid check-in or check-out date");
        }
        if (user.PhoneNumber == null)
        {
            user.PhoneNumber = bookingRequest.PhoneNumber;
            await _userRepository.UpdateUserAsync(user);
        }
        var booking = _mapper.Map<Booking>(bookingRequest);
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
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null)
        {
            throw new CustomException(ErrorCode.NotFound, "Booking not found");
        }
        if (booking.BookingStatus == API.Enum.BookingStatus.Cancelled)
        {
            throw new CustomException(ErrorCode.BadRequest, "Booking already cancelled");
        }
        if (booking.CheckInDate < DateTime.Now)
        {
            throw new CustomException(ErrorCode.BadRequest, "Cannot cancel booking after check-in date");
        }
        return await _bookingRepository.CancelBookingAsync(bookingId);
    }
}
