using API.Models;

public interface IRoomRepository
{
    Task<List<Room>> GetAllRoomsAsync(int pageNumber, int pageSize);
    Task<List<Room>> GetAllRoomsNoPagingAsync();
    Task<int> GetTotalRoomsCountAsync();
    Task<Room?> GetRoomByIDAsync(int id);
    Task<List<Room>> GetRoomsByTypeAsync(string roomType);
    Task<Room> AddRoomAsync(Room room);
    Task<Room?> UpdateRoomAsync(Room room);
    Task<Room?> DeleteRoomAsync(int id);
    Task<List<RoomImage>> GetRoomImagesAsync(int roomId);
    Task AddRoomImagesAsync(List<RoomImage> images);
    Task DeleteRoomImagesAsync(int roomId);
    Task<List<Room>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, string roomType);
    Task<List<Room>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate);
    Task<Room?> GetAvailableRoomAsync(int id, DateTime checkInDate, DateTime checkOutDate);
    Task<List<string>> GetRoomImageUrlsAsync(int roomId);
}
