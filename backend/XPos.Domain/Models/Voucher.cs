using System;

namespace XPos.Domain.Models;

public class Voucher
{
    public long Id { get; set; }
    public long? SaleId { get; set; }
    public Sale? Sale { get; set; }
    public long? PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public long? SaleReturnId { get; set; }
    public SaleReturn? SaleReturn { get; set; }
    public long? PurchaseReturnId { get; set; }
    public PurchaseReturn? PurchaseReturn { get; set; }
    public string VoucherType { get; set; } = string.Empty;
    public string VoucherNumber { get; set; } = string.Empty;
    public string Cae { get; set; } = string.Empty;
    public DateTime CaeExpiration { get; set; }
    public DateTime IssuedAt { get; set; }
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }
}
