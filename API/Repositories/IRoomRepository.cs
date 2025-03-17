using API.Models;

public interface IRoomRepository
{
    Task<IEnumerable<Room>> GetAllRoomAsync();
    Task<Room?> GetRoomByIDAsync(int id);
    Task<Room> AddRoomAsync(Room room);
    Task<Room?> UpdateRoomAsync(Room room);
    Task<Room?> DeleteRoomAsync(int id);
}