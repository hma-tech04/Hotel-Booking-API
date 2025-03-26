using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

public class RoomRepository : IRoomRepository
{
    private readonly HotelBookingContext _context;

    public RoomRepository(HotelBookingContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Room>> GetAllRoomAsync()
    {
        return await _context.Rooms.ToListAsync();
    }

    public async Task<Room?> GetRoomByIDAsync(int id)
    {
        return await _context.Rooms.FindAsync(id);
    }

    public async Task<Room> AddRoomAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room?> UpdateRoomAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room?> DeleteRoomAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return null;
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return room;
    }
}