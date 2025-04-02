using API.Data;
using API.DTOs.Statistics;
using API.Enum;
using API.Models;
using Microsoft.EntityFrameworkCore;

public class PaymentRepository : IPaymentRepository
{
    private readonly HotelBookingContext _context;
    public PaymentRepository(HotelBookingContext context)
    {
        _context = context;
    }

    public async Task<Payment> CreatePayment(Payment payment)
    {
        await _context.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }
    public async Task<Payment?> GetPaymentById(int paymentId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
    }

    public async Task<List<Payment>> GetPaymentsByBookingId(int bookingId)
    {
        return await _context.Payments
            .Where(p => p.BookingId == bookingId)
            .ToListAsync();
    }
    public async Task<bool> UpdatePaymentStatus(int paymentId, PaymentStatus paymentStatus)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null)
            return false;
        payment.PaymentStatus = paymentStatus;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> RetrieveMonthlyRevenue(MonthlyRevenueRequestDto dto)
    {
        var paymentStats = await _context.Payments
            .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value.Year == dto.Year && p.PaymentDate.Value.Month == dto.Month && p.PaymentStatus == PaymentStatus.Completed)  
            .SumAsync(p => p.PaymentAmount);
        return paymentStats;
    }

    public async Task<decimal> RetrieveToltalRevenue(){
        return await _context.Payments
            .Where(p => p.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(p => p.PaymentAmount);
    }
}