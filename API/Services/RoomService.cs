using API.DTOs;
using API.Models;
using API.Repositories;
using AutoMapper;
namespace API.Services;

public class RoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;

    public RoomService(IRoomRepository roomRepository, IMapper mapper)
    {
        _roomRepository = roomRepository;
        _mapper = mapper;
    }

    // Lấy danh sách phòng
    public async Task<IEnumerable<RoomDTO>> GetAllRoomsAsync()
    {
        var rooms = await _roomRepository.GetAllRoomAsync();
        return rooms.Select(room => new RoomDTO
        {
            RoomId = room.RoomId,
            RoomType = room.RoomType,
            Price = room.Price,
            Description = room.Description,
            ThumbnailUrl = room.ThumbnailUrl,
            IsAvailable = room.IsAvailable,
            RoomImages = room.RoomImages.Select(img => img.ImageUrl).ToList()
        });
    }

    // Lấy phòng theo ID
    public async Task<RoomDTO> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetRoomByIDAsync(id);
        if (room == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {id}");
        }

        return new RoomDTO
        {
            RoomId = room.RoomId,
            RoomType = room.RoomType,
            Price = room.Price,
            Description = room.Description,
            ThumbnailUrl = room.ThumbnailUrl,
            IsAvailable = room.IsAvailable,
            RoomImages = room.RoomImages.Select(img => img.ImageUrl).ToList()
        };
    }

    // Thêm phòng mới
    public async Task<RoomDTO> AddRoomAsync(RoomDTO roomDTO)
    {
        var room = _mapper.Map<Room>(roomDTO);
        var newRoom = await _roomRepository.AddRoomAsync(room);
        return _mapper.Map<RoomDTO>(newRoom);
    }

    // Cập nhật thông tin phòng
    public async Task<RoomDTO> UpdateRoomAsync(RoomDTO roomDTO)
    {
        var existingRoom = await _roomRepository.GetRoomByIDAsync(roomDTO.RoomId);
        if (existingRoom == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {roomDTO.RoomId}");
        }

        var roomToUpdate = _mapper.Map<Room>(roomDTO);
        var updatedRoom = await _roomRepository.UpdateRoomAsync(roomToUpdate);
        return _mapper.Map<RoomDTO>(updatedRoom);
    }

    // Xóa phòng
    public async Task<RoomDTO> DeleteRoomAsync(int id)
    {
        var deletedRoom = await _roomRepository.DeleteRoomAsync(id);
        if (deletedRoom == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {id}");
        }
        return _mapper.Map<RoomDTO>(deletedRoom);
    }
}
