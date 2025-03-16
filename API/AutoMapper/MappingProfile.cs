using AutoMapper;
using API.Models;
using API.DTOs;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDTO>();
        CreateMap<Booking, BookingDTO>().ReverseMap();
        CreateMap<Room, RoomDTO>().ReverseMap();
        CreateMap<Payment, PaymentDTO>().ReverseMap();
        CreateMap<RoomImage, RoomImageDTO>().ReverseMap();
        CreateMap<UserRegisterDTO, User>();
    }
} 