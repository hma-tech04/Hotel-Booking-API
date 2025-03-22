using API.DTOs.Response;
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
    public async Task<PagedResponse<RoomDTO>> GetAllRoomsAsync(int pageNumber, int pageSize)
    {
        var rooms = await _roomRepository.GetAllRoomsAsync(pageNumber, pageSize);
        int totalRecords = await _roomRepository.GetTotalRoomsCountAsync();

        var roomDTOs = rooms.Select(room => new RoomDTO
        {
            RoomId = room.RoomId,
            RoomType = room.RoomType,
            Price = room.Price,
            Description = room.Description,
            ThumbnailUrl = room.ThumbnailUrl,
            IsAvailable = room.IsAvailable,
            RoomImages = room.RoomImages.Select(img => img.ImageUrl).ToList()
        });

        return new PagedResponse<RoomDTO>(roomDTOs, pageNumber, pageSize, totalRecords);
    }
    public async Task<List<RoomDTO>> GetAllRoomsNoPagingAsync()
    {
        var rooms = await _roomRepository.GetAllRoomsNoPagingAsync();
        var roomDTOs = rooms.Select(room => new RoomDTO
        {
            RoomId = room.RoomId,
            RoomType = room.RoomType,
            Price = room.Price,
            Description = room.Description,
            ThumbnailUrl = room.ThumbnailUrl,
            IsAvailable = room.IsAvailable,
            RoomImages = room.RoomImages.Select(img => img.ImageUrl).ToList()
        }).ToList();
        return roomDTOs;
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
    public async Task<List<RoomDTO>> GetRoomsByTypeAsync(string roomType)
    {
        var rooms = await _roomRepository.GetRoomsByTypeAsync(roomType);
        return rooms.Select(room => new RoomDTO
        {
            RoomId = room.RoomId,
            RoomType = room.RoomType,
            Price = room.Price,
            Description = room.Description,
            ThumbnailUrl = room.ThumbnailUrl,
            IsAvailable = room.IsAvailable,
            RoomImages = room.RoomImages.Select(img => img.ImageUrl).ToList()
        }).ToList();
    }

    public async Task<RoomDTO> AddRoomAsync(RoomDTO roomDTO, IFormFile? imageFile = null)
    {
        if (roomDTO == null)
            throw new ArgumentNullException(nameof(roomDTO), "RoomDTO cannot be null");

        if (roomDTO.Price <= 0)
            throw new ArgumentException("Price must be greater than 0.", nameof(roomDTO.Price));

        var room = new Room
        {
            RoomType = roomDTO.RoomType,
            Price = roomDTO.Price,
            Description = roomDTO.Description,
            IsAvailable = roomDTO.IsAvailable,
            ThumbnailUrl = null
        };

        if (imageFile != null)
        {
            var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var uploadPath = Path.Combine(_env.WebRootPath, "images");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            room.ThumbnailUrl = $"/images/{fileName}";
        }

        var newRoom = await _roomRepository.AddRoomAsync(room);
        return _mapper.Map<RoomDTO>(newRoom);
    }

    public async Task<RoomDTO> UpdateRoomAsync(int id, RoomDTO roomDTO, IFormFile? imageFile = null)
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

        if (imageFile != null)
        {
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
