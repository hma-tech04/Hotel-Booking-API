using API.DTOs.EntityDTOs;
using API.DTOs.Request;
using API.Models;
using API.Repositories;
using AutoMapper;
using API.Enum;

public class BookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public BookingService(IBookingRepository bookingRepository, IMapper mapper, IRoomRepository roomRepository, IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<BookingDTO> AddBookingAsync(BookingRequest bookingRequest)
    {
        if (bookingRequest == null)
            throw new CustomException(ErrorCode.BadRequest, "Invalid booking request");

        var user = await _userRepository.GetUserByIDAsync(bookingRequest.UserId)
            ?? throw new CustomException(ErrorCode.NotFound, "User not found");

        var room = await _roomRepository.GetRoomByIDAsync(bookingRequest.RoomId)
            ?? throw new CustomException(ErrorCode.NotFound, "Room not found");

        if (room.IsAvailable != true)
            throw new CustomException(ErrorCode.BadRequest, "Room is not available");

        if (bookingRequest.CheckInDate < DateTime.Now)
            throw new CustomException(ErrorCode.BadRequest, "Check-in date cannot be in the past");

        if (bookingRequest.CheckOutDate <= bookingRequest.CheckInDate)
            throw new CustomException(ErrorCode.BadRequest, "Check-out date must be after check-in date");

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            user.PhoneNumber = bookingRequest.PhoneNumber;
            await _userRepository.UpdateUserAsync(user);
        }

        var booking = _mapper.Map<Booking>(bookingRequest);
        booking.BookingStatus = BookingStatus.Confirmed;

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
        return booking != null ? _mapper.Map<BookingDTO>(booking) : null;
    }

    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
            ?? throw new CustomException(ErrorCode.NotFound, "Booking not found");

        if (booking.BookingStatus == BookingStatus.Cancelled)
            throw new CustomException(ErrorCode.BadRequest, "Booking is already cancelled");

        if (booking.CheckInDate < DateTime.Now)
            throw new CustomException(ErrorCode.BadRequest, "Cannot cancel booking after check-in date");

        return await _bookingRepository.CancelBookingAsync(bookingId);
    }
}
