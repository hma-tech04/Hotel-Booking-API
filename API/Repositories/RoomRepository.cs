using API.Data;
using API.Enum;
using API.Models;
using Microsoft.Data.SqlClient;
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
                .Include(r => r.RoomImages)
                .Where(r => r.IsAvailable == true)
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
            if (images != null && images.Any())
            {
                var query = "INSERT INTO RoomImages (RoomId, ImageUrl) VALUES (@RoomId, @ImageUrl)";
                foreach (var image in images)
                {
                    await _context.Database.ExecuteSqlRawAsync(query,
                        new SqlParameter("@RoomId", image.RoomId),
                        new SqlParameter("@ImageUrl", image.ImageUrl));
                }
            }
        }

        public async Task DeleteRoomImagesAsync(int roomId)
        {
            string query = "DELETE FROM RoomImages WHERE RoomId = @RoomID";
            await _context.Database.ExecuteSqlRawAsync(query,
                new SqlParameter("@RoomID", roomId)
            );
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, string roomType)
        {
            return await _context.Rooms
                .Include(r => r.Bookings)
                .Include(r => r.RoomImages)
                .Where(r => r.IsAvailable == true
                            && r.RoomType == roomType
                            && !r.Bookings.Any(b => checkInDate < b.CheckOutDate && checkOutDate > b.CheckInDate)) // Tránh xung đột đặt phòng
                .ToListAsync();
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate)
        {
            return await _context.Rooms
                .Include(r => r.Bookings)
                .Include(r => r.RoomImages)
                .Where(r => r.IsAvailable == true &&
                            !r.Bookings.Any(b =>
                                (b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Pending) &&
                                checkInDate < b.CheckOutDate &&
                                checkOutDate > b.CheckInDate))
                .ToListAsync();
        }

        public async Task<Room?> GetAvailableRoomsAsync(int id, DateTime checkInDate, DateTime checkOutDate)
        {
            return await _context.Rooms
                .Include(r => r.Bookings)
                .Where(r => r.RoomId == id)
                .FirstOrDefaultAsync(r => !r.Bookings.Any(b => checkInDate < b.CheckOutDate && checkOutDate > b.CheckInDate));
        }
        public async Task<List<string>> GetRoomImageUrlsAsync(int roomId)
        {
            return await _context.RoomImages
                                 .Where(img => img.RoomId == roomId)
                                 .Select(img => img.ImageUrl)
                                 .ToListAsync();
        }


    }
}
