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

        // Chuyển `RoomRequestDTO` thành `RoomDTO`
        var roomDTO = new RoomDTO
        {
            RoomType = roomRequestDTO.RoomType,
            ThumbnailUrl = await SaveImageAsync(roomRequestDTO.ThumbnailUrl),
            RoomImages = new List<string>(),
            Price = roomRequestDTO.Price,
            Description = roomRequestDTO.Description,
            IsAvailable = roomRequestDTO.IsAvailable ?? true // Mặc định là có sẵn nếu null
        };

        var room = _mapper.Map<Room>(roomDTO);
        var newRoom = await _roomRepository.AddRoomAsync(room);

        if (imageFiles != null && imageFiles.Any())
        {
            var roomImages = await SaveImagesAsync(newRoom.RoomId, imageFiles);
            await _roomRepository.AddRoomImagesAsync(roomImages);

            newRoom.ThumbnailUrl = roomImages.First().ImageUrl;
            await _roomRepository.UpdateRoomAsync(newRoom);

            roomDTO.RoomImages = roomImages.Select(img => img.ImageUrl).ToList();
            roomDTO.ThumbnailUrl = newRoom.ThumbnailUrl;
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

            // Save the images and check if there are any
            var roomImages = imageFiles != null ? await SaveImagesAsync(id, imageFiles) : new List<RoomImage>();

            // Ensure roomDTO is properly created with the room images
            var roomDTO = new RoomDTO
            {
                RoomType = roomRequestDTO.RoomType,
                ThumbnailUrl = await SaveImageAsync(roomRequestDTO.ThumbnailUrl),
                RoomImages = roomImages.Select(image => image.ImageUrl).ToList(),
                Price = roomRequestDTO.Price,
                Description = roomRequestDTO.Description,
                IsAvailable = roomRequestDTO.IsAvailable ?? true // Default to true if null
            };

            // Update existing room with new data
            existingRoom = _mapper.Map<Room>(roomDTO);
            existingRoom.RoomId = id;

            // Delete old room images and add the new ones
            await _roomRepository.DeleteRoomImagesAsync(id);

            if (roomImages.Any())
            {
                // Ensure that thumbnail is set if there are room images
                existingRoom.ThumbnailUrl = roomImages.First().ImageUrl;
                await _roomRepository.AddRoomImagesAsync(roomImages);
            }

            var updatedRoom = await _roomRepository.UpdateRoomAsync(existingRoom);
            if (updatedRoom == null)
            {
                throw new CustomException(ErrorCode.NotFound, $"No room found with ID: {id}");
            }

            await transaction.CommitAsync();

            return _mapper.Map<RoomDTO>(updatedRoom);
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
    public async Task<List<RoomDTO>> GetAvailableRoomsAsync()
    {
        var rooms = await _roomRepository.GetAvailableRoomsAsync();
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