using API.DTOs.EntityDTOs;
using API.DTOs.Request;
using API.Models;
using API.Repositories;
using AutoMapper;
using API.Enum;
using Microsoft.Identity.Client;
using DTOs.Statistics;
using API.DTOs.Statistics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class BookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly PaymentService _paymentService;

    public BookingService(PaymentService paymentService, IBookingRepository bookingRepository, IMapper mapper, IRoomRepository roomRepository, IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _paymentService = paymentService;
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

        int Days = (bookingRequest.CheckOutDate - bookingRequest.CheckInDate).Days;
        decimal Price = room.Price;
        decimal TotalPrice = Price * Days;
        bookingRequest.TotalPrice = TotalPrice;

        var booking = _mapper.Map<Booking>(bookingRequest);

        booking.BookingStatus = BookingStatus.Pending;

        var newBooking = await _bookingRepository.AddBookingAsync(booking);
        return _mapper.Map<BookingDTO>(newBooking);
    }

    public async Task<List<BookingDTO>> GetBookingsByUserIdAsync(int userId)
    {
        var user = await _userRepository.GetUserByIDAsync(userId);
        if(user == null){
            throw new CustomException(ErrorCode.NotFound, $"User not found with ID {userId}");
        }
        var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
        return _mapper.Map<List<BookingDTO>>(bookings);
    }

    public async Task<BookingDTO?> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if(booking == null){
            throw new CustomException(ErrorCode.NotFound, $"Booking not found with {bookingId}");
        }
        return _mapper.Map<BookingDTO>(booking);
    }

    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
            ?? throw new CustomException(ErrorCode.NotFound, "Booking not found");

        if (booking.BookingStatus == BookingStatus.Cancelled)
            throw new CustomException(ErrorCode.BadRequest, "Booking is already cancelled");

        // if (booking.CheckInDate < DateTime.Now)
        //     throw new CustomException(ErrorCode.BadRequest, "Cannot cancel booking after check-in date");

        var payments = await _paymentService.GetPaymentStatusByBookingId(bookingId);
        foreach (var item in payments)
        {
            await _paymentService.UpdatePaymentStatus(item.PaymentId, PaymentStatus.Cancelled);
        }
        return await _bookingRepository.CancelBookingAsync(bookingId);
    }

    public async Task<bool> UpdateBookingStatusAsync(int bookingId,  BookingStatus status)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
            ?? throw new CustomException(ErrorCode.NotFound, "Booking not found");

        if (booking.BookingStatus == BookingStatus.Cancelled)
            throw new CustomException(ErrorCode.BadRequest, "Cannot update a cancelled booking");

        booking.BookingStatus = status;
        return await _bookingRepository.UpdateBookingAsync(booking);
    }

    public async Task<List<BookingDTO>> GetAllBookingAsync(){
        var bookings = await _bookingRepository.GetAllBooking();
        return _mapper.Map<List<BookingDTO>>(bookings);
    }

    public async Task<bool> CheckInBooking(int bookingId){
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if(booking == null){
            throw new CustomException(ErrorCode.NotFound, $"Booking not found with {bookingId}");
        }

        if(booking.PaymentId == null){
            throw new CustomException(ErrorCode.BadRequest, $"Please payment booking before proceeding to check in.");
        }
        booking.ActualCheckInTime = DateTime.Now;
        var bookingCheckedIn = await _bookingRepository.UpdateBookingAsync(booking);
        return bookingCheckedIn;
    }

    public async Task<bool> CheckOutBooking(int bookingId){
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if(booking == null){
            throw new CustomException(ErrorCode.NotFound, $"Booking not found with {bookingId}");
        }
        if(booking.ActualCheckInTime == null){
            throw new CustomException(ErrorCode.BadRequest, "Please check in before proceeding to check out.");
        }
        booking.ActualCheckOutTime = DateTime.Now;
        booking.BookingStatus = BookingStatus.Completed;
        var bookingCheckedIn = await _bookingRepository.UpdateBookingAsync(booking);
        return bookingCheckedIn;
    }

    public async Task<List<BookingDTO>> RetrieveBookingStats(DateRangeDto dateRangeDto){
        var result = await _bookingRepository.RetrieveBookingStats(dateRangeDto.StartDate, dateRangeDto.EndDate);
        return _mapper.Map<List<BookingDTO>>(result);
    }

    public async Task<List<BookingDTO>> GetUncheckedInBookingsByPhoneNumber(PhoneNumberRequestDto requestDto){
        var result = await _bookingRepository.GetUncheckedInBookingsByPhoneNumber(requestDto);
        return _mapper.Map<List<BookingDTO>>(result);
    }

}
