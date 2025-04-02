using AutoMapper;
using API.Models;
using API.DTOs.Auth;
using API.DTOs.Request;
using API.DTOs.EntityDTOs;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        CreateMap<Booking, BookingDTO>().ReverseMap();

        CreateMap<Room, RoomDTO>()
            .ForMember(dest => dest.RoomImages, opt => opt.MapFrom(src => src.RoomImages.Select(img => img.ImageUrl).ToList()))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable)); 


        CreateMap<RoomDTO, Room>()
            .ForMember(dest => dest.RoomImages, opt => opt.Ignore())
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable));

        CreateMap<Payment, PaymentDTO>().ReverseMap();
        CreateMap<RoomImage, RoomImageDTO>().ReverseMap();
        CreateMap<UserRegisterDTO, User>();
        CreateMap<BookingRequest, Booking>();
    }
}