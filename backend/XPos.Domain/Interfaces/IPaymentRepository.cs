using System;
using System.Threading.Tasks;

namespace XPos.Domain.Interfaces;

public class PaymentSaleDto
{
    public long? UserId { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public long SaleId { get; set; }
    public decimal Amount { get; set; }
    public string Reglement { get; set; } = string.Empty;
    public long? CreatedBy { get; set; }
}

public class PaymentPurchaseDto
{
    public long? UserId { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public long PurchaseId { get; set; }
    public decimal Amount { get; set; }
    public string Reglement { get; set; } = string.Empty;
    public long? CreatedBy { get; set; }
}

public interface IPaymentRepository
{
    Task<long> CreateSalePaymentAsync(PaymentSaleDto payment);
    Task<long> CreatePurchasePaymentAsync(PaymentPurchaseDto payment);
}
