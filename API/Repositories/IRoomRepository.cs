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
}
