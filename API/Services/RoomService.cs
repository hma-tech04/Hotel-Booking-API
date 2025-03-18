using API.DTOs;
using API.Models;
using API.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

public class RoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    public RoomService(IRoomRepository roomRepository, IMapper mapper, IWebHostEnvironment env)
    {
        _roomRepository = roomRepository;
        _mapper = mapper;
        _env = env;
    }

    // Lấy danh sách phòng (bao gồm danh sách ảnh)
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

    // Lấy phòng theo ID (bao gồm danh sách ảnh)
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

    // Thêm phòng (hỗ trợ lưu ảnh vào wwwroot/images)
    public async Task<RoomDTO> AddRoomAsync(RoomDTO roomDTO, IFormFile? imageFile)
    {
        var room = _mapper.Map<Room>(roomDTO);

        // Lưu ảnh nếu có file tải lên
        if (imageFile != null)
        {
            var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            room.ThumbnailUrl = $"/images/{fileName}";
        }

        var newRoom = await _roomRepository.AddRoomAsync(room);
        return await GetRoomByIdAsync(newRoom.RoomId); // Lấy dữ liệu có đầy đủ danh sách ảnh
    }

    // Cập nhật phòng
    public async Task<RoomDTO> UpdateRoomAsync(int id, RoomDTO roomDTO, IFormFile? imageFile)
    {
        var existingRoom = await _roomRepository.GetRoomByIDAsync(id);
        if (existingRoom == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {id}");
        }

        // Cập nhật thông tin phòng
        existingRoom.RoomType = roomDTO.RoomType;
        existingRoom.Price = roomDTO.Price;
        existingRoom.Description = roomDTO.Description;
        existingRoom.IsAvailable = roomDTO.IsAvailable;

        // Cập nhật ảnh nếu có file mới tải lên
        if (imageFile != null)
        {
            // Xóa ảnh cũ nếu tồn tại
            if (!string.IsNullOrEmpty(existingRoom.ThumbnailUrl))
            {
                var oldImagePath = Path.Combine(_env.WebRootPath, existingRoom.ThumbnailUrl.TrimStart('/'));
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }
            }

            // Lưu ảnh mới
            var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            existingRoom.ThumbnailUrl = $"/images/{fileName}";
        }

        var updatedRoom = await _roomRepository.UpdateRoomAsync(existingRoom);
        return await GetRoomByIdAsync(updatedRoom.RoomId);
    }

    // Xóa phòng
    public async Task<RoomDTO> DeleteRoomAsync(int id)
    {
        var deletedRoom = await _roomRepository.DeleteRoomAsync(id);
        if (deletedRoom == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {id}");
        }

        // Xóa ảnh khỏi thư mục
        if (!string.IsNullOrEmpty(deletedRoom.ThumbnailUrl))
        {
            var imagePath = Path.Combine(_env.WebRootPath, deletedRoom.ThumbnailUrl.TrimStart('/'));
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }

        return _mapper.Map<RoomDTO>(deletedRoom);
    }
}
