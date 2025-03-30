using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly HotelBookingContext _context;

        public RoomRepository(HotelBookingContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetAllRoomsAsync(int pageNumber, int pageSize)
        {
            return await _context.Rooms
                .Include(r => r.RoomImages)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Room>> GetAllRoomsNoPagingAsync()
        {
            return await _context.Rooms
                .Include(r => r.RoomImages)
                .ToListAsync();
        }

        public async Task<int> GetTotalRoomsCountAsync()
        {
            return await _context.Rooms.CountAsync();
        }

        public async Task<Room?> GetRoomByIDAsync(int id)
        {
            return await _context.Rooms.Include(r => r.RoomImages)
                                       .FirstOrDefaultAsync(r => r.RoomId == id);
        }

        public async Task<List<Room>> GetRoomsByTypeAsync(string roomType)
        {
            return await _context.Rooms
                .Where(r => r.RoomType == roomType)
                .ToListAsync();
        }

        public async Task<Room> AddRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Room?> UpdateRoomAsync(Room room)
        {
            var existingRoom = await _context.Rooms.Include(r => r.RoomImages)
                                                   .FirstOrDefaultAsync(r => r.RoomId == room.RoomId);
            if (existingRoom == null)
                return null;

            _context.Entry(existingRoom).CurrentValues.SetValues(room);
            await _context.SaveChangesAsync();
            return existingRoom;
        }

        public async Task<Room?> DeleteRoomAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return null;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<List<RoomImage>> GetRoomImagesAsync(int roomId)
        {
            return await _context.RoomImages.Where(img => img.RoomId == roomId).ToListAsync();
        }

        public async Task AddRoomImagesAsync(List<RoomImage> images)
        {
            _context.RoomImages.AddRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoomImagesAsync(int roomId)
        {
            var images = await _context.RoomImages.Where(img => img.RoomId == roomId).ToListAsync();
            _context.RoomImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, string roomType)
        {
            return await _context.Rooms
                .Include(r => r.Bookings)
                .Where(r => r.IsAvailable == true
                            && r.RoomType == roomType
                            && !r.Bookings.Any(b => checkInDate < b.CheckOutDate && checkOutDate > b.CheckInDate)) // Tránh xung đột đặt phòng
                .ToListAsync();
        }

        public async Task<List<Room>> GetAvailableRoomsAsync()
        {
            return await _context.Rooms
                .Include(r => r.RoomImages)
                .Where(r => r.IsAvailable == true)
                .ToListAsync();
        }
        public async Task<Room?> GetAvailableRoomsAsync(int id, DateTime checkInDate, DateTime checkOutDate)
        {
            return await _context.Rooms
                .Include(r => r.Bookings)
                .Where(r => r.RoomId == id && r.IsAvailable == true)
                .FirstOrDefaultAsync(r => !r.Bookings.Any(b => checkInDate < b.CheckOutDate && checkOutDate > b.CheckInDate));
        }

    }
}
