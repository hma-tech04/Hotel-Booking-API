using API.DTOs.Statistics;
using API.Enum;
using API.Models;
using VNPAY.NET.Models;

public interface IPaymentRepository{
    Task<Payment> CreatePayment(Payment payment);
    Task<Payment?> GetPaymentById(int paymentId);
    Task<List<Payment>> GetPaymentsByBookingId(int bookingId);
    Task<bool> UpdatePaymentStatus(int paymentId, PaymentStatus paymentStatus);
    Task<decimal> RetrieveMonthlyRevenue(MonthlyRevenueRequestDto monthlyRevenueRequestDto);
    Task<decimal> RetrieveToltalRevenue();
}