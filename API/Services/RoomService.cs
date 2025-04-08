using API.DTOs.Response;
using API.Models;
using AutoMapper;
using API.DTOs.EntityDTOs;
using API.Data;
public class RoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;
    private readonly HotelBookingContext _context;

    public RoomService(HotelBookingContext context, IRoomRepository roomRepository, IMapper mapper, IWebHostEnvironment env)
    {
        _roomRepository = roomRepository;
        _mapper = mapper;
        _env = env;
        _context = context;
    }

    // Get all rooms with pagination
    public async Task<PagedResponse<RoomDTO>> GetAllRoomsAsync(int pageNumber, int pageSize)
    {
        var rooms = await _roomRepository.GetAllRoomsAsync(pageNumber, pageSize);
        int totalRecords = await _roomRepository.GetTotalRoomsCountAsync();
        var roomDTOs = _mapper.Map<List<RoomDTO>>(rooms);
        return new PagedResponse<RoomDTO>(roomDTOs, pageNumber, pageSize, totalRecords);
    }

    // Get all rooms without pagination
    public async Task<List<RoomDTO>> GetAllRoomsNoPagingAsync()
    {
        var rooms = await _roomRepository.GetAllRoomsNoPagingAsync();
        return _mapper.Map<List<RoomDTO>>(rooms);
    }

    // Get room by ID
    public async Task<RoomDTO> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetRoomByIDAsync(id);
        if (room == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {id}");
        }
        return _mapper.Map<RoomDTO>(room);
    }

    // Get rooms by type
    public async Task<List<RoomDTO>> GetRoomsByTypeAsync(string roomType)
    {
        var rooms = await _roomRepository.GetRoomsByTypeAsync(roomType);
        return _mapper.Map<List<RoomDTO>>(rooms);
    }

    // Add new room
    public async Task<RoomDTO> AddRoomAsync(RoomRequestDTO roomRequestDTO, List<IFormFile>? imageFiles)
    {
        ValidateRoomRequestDTO(roomRequestDTO);

        string thumbnailUrl = null;
        List<string> roomImageUrls = new();

        if (roomRequestDTO.ThumbnailUrl != null)
        {
            thumbnailUrl = await SaveImageAsync(roomRequestDTO.ThumbnailUrl);
        }

        if (imageFiles != null && imageFiles.Any())
        {
            var savedImages = await SaveImagesAsync(0, imageFiles);
            roomImageUrls = savedImages.Select(img => img.ImageUrl).ToList();

            if (string.IsNullOrEmpty(thumbnailUrl))
            {
                thumbnailUrl = roomImageUrls.First();
            }
        }

        if (string.IsNullOrEmpty(thumbnailUrl))
        {
            throw new CustomException(ErrorCode.InvalidData, "You must provide at least one image or a thumbnail.");
        }

        var roomDTO = new RoomDTO
        {
            RoomType = roomRequestDTO.RoomType,
            Price = roomRequestDTO.Price,
            Description = roomRequestDTO.Description,
            IsAvailable = roomRequestDTO.IsAvailable ?? true,
            ThumbnailUrl = thumbnailUrl,
            RoomImages = roomImageUrls
        };

        var room = _mapper.Map<Room>(roomDTO);
        var newRoom = await _roomRepository.AddRoomAsync(room);

        // Lưu lại RoomImages chính xác nếu có
        if (imageFiles != null && imageFiles.Any())
        {
            var savedImages = await SaveImagesAsync(newRoom.RoomId, imageFiles);
            await _roomRepository.AddRoomImagesAsync(savedImages);
            roomDTO.RoomImages = savedImages.Select(i => i.ImageUrl).ToList();
        }

        return roomDTO;
    }



    // Validate RoomRequestDTO properties
    private void ValidateRoomRequestDTO(RoomRequestDTO roomRequestDTO)
    {
        if (roomRequestDTO == null)
        {
            throw new ArgumentNullException(nameof(roomRequestDTO), "RoomRequestDTO cannot be null");
        }

        if (roomRequestDTO.Price <= 0)
        {
            throw new ArgumentException("Price must be greater than 0.", nameof(roomRequestDTO.Price));
        }
    }

    // Update existing room
    public async Task<RoomDTO> UpdateRoomAsync(int id, RoomRequestDTO roomRequestDTO, List<IFormFile>? imageFiles)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingRoom = await _roomRepository.GetRoomByIDAsync(id);
            if (existingRoom == null)
            {
                throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {id}");
            }

            string thumbnailUrl = existingRoom.ThumbnailUrl;

            if (roomRequestDTO.ThumbnailUrl != null)
            {
                thumbnailUrl = await SaveImageAsync(roomRequestDTO.ThumbnailUrl);
            }

            List<RoomImage> roomImages = new List<RoomImage>();
            if (imageFiles != null && imageFiles.Any())
            {
                await _roomRepository.DeleteRoomImagesAsync(id);

                roomImages = await SaveImagesAsync(id, imageFiles);

                await _roomRepository.AddRoomImagesAsync(roomImages);
            }

            existingRoom.RoomType = roomRequestDTO.RoomType;
            existingRoom.Price = roomRequestDTO.Price;
            existingRoom.Description = roomRequestDTO.Description;
            existingRoom.IsAvailable = roomRequestDTO.IsAvailable ?? true;
            existingRoom.ThumbnailUrl = thumbnailUrl;

            var updatedRoom = await _roomRepository.UpdateRoomAsync(existingRoom);
            await transaction.CommitAsync();

            var roomDTO = _mapper.Map<RoomDTO>(updatedRoom);
            roomDTO.RoomImages = roomImages.Any()
                ? roomImages.Select(img => img.ImageUrl).ToList()
                : await _roomRepository.GetRoomImageUrlsAsync(id);

            return roomDTO;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }



    // Save thumbnail image asynchronously
    private async Task<string> SaveImageAsync(IFormFile thumbnailUrl)
    {
        if (thumbnailUrl == null)
        {
            throw new ArgumentNullException(nameof(thumbnailUrl), "Thumbnail image cannot be null.");
        }

        var fileName = $"{Guid.NewGuid()}_{thumbnailUrl.FileName}";
        var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

        // Save the file asynchronously
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await thumbnailUrl.CopyToAsync(stream);
        }

        return $"/images/{fileName}";
    }

    // Delete room
    public async Task<RoomDTO> DeleteRoomAsync(int id)
    {
        var deletedRoom = await _roomRepository.DeleteRoomAsync(id);
        if (deletedRoom == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {id}");
        }

        if (!string.IsNullOrEmpty(deletedRoom.ThumbnailUrl))
        {
            var imagePath = Path.Combine(_env.WebRootPath, deletedRoom.ThumbnailUrl.TrimStart('/'));
            try
            {
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }
            catch (Exception)
            {
                throw new CustomException(ErrorCode.InternalError, "Error deleting image file.");
            }
        }

        return _mapper.Map<RoomDTO>(deletedRoom);
    }

    // Get available rooms (no date filtering)
    public async Task<List<RoomDTO>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate)
    {
        var rooms = await _roomRepository.GetAvailableRoomsAsync(checkInDate, checkOutDate);
        return _mapper.Map<List<RoomDTO>>(rooms);
    }

    // Get available rooms with date filtering
    public async Task<List<RoomDTO>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, string roomType)
    {
        var rooms = await _roomRepository.GetAvailableRoomsAsync(checkInDate, checkOutDate, roomType);
        return _mapper.Map<List<RoomDTO>>(rooms);
    }

    // Check if room is available within date range
    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        var room = await _roomRepository.GetRoomByIDAsync(roomId);
        if (room == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {roomId}");
        }

        var availableRoom = await _roomRepository.GetAvailableRoomsAsync(roomId, checkInDate, checkOutDate);
        return availableRoom != null;
    }

    // Save multiple images asynchronously
    private async Task<List<RoomImage>> SaveImagesAsync(int roomId, List<IFormFile> imageFiles)
    {
        var roomImages = new List<RoomImage>();
        foreach (var imageFile in imageFiles)
        {
            ValidateImage(imageFile); // Validate image before saving

            var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var imageUrl = $"/images/{fileName}";
            roomImages.Add(new RoomImage { RoomId = roomId, ImageUrl = imageUrl });
        }

        return roomImages;
    }

    // Validate RoomDTO properties
    private void ValidateRoomDTO(RoomDTO roomDTO)
    {
        if (roomDTO == null)
        {
            throw new ArgumentNullException(nameof(roomDTO), "RoomDTO cannot be null");
        }

        if (roomDTO.Price <= 0)
        {
            throw new ArgumentException("Price must be greater than 0.", nameof(roomDTO.Price));
        }
    }

    // Validate image properties (size, type)
    private void ValidateImage(IFormFile image)
    {
        if (image.Length == 0)
        {
            throw new CustomException(ErrorCode.InvalidData, "Image file cannot be empty.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(image.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new CustomException(ErrorCode.InvalidData, "Invalid image format. Allowed formats: .jpg, .jpeg, .png.");
        }
    }
}