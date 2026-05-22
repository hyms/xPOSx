using System;

namespace XPos.Domain.Models;

public class CashTransaction
{
    public long Id { get; set; }
    public long CashShiftId { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty; // "FLOAT_IN", "FLOAT_OUT", "CASH_DROP", "EXPENSE"
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? RecipientName { get; set; }
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
