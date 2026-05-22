using System;

namespace XPos.Domain.Dtos;

public class OpenShiftDto
{
    public long RegisterId { get; set; }
    public decimal StartingCash { get; set; }
}

public class CloseShiftDto
{
    public long ShiftId { get; set; }
    public decimal ActualCash { get; set; }
    public string? Notes { get; set; }
}

public class CashTransactionDto
{
    public long ShiftId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // "FLOAT_IN", "FLOAT_OUT", "CASH_DROP", "EXPENSE"
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? RecipientName { get; set; }
}

public class ActiveShiftDto
{
    public long ShiftId { get; set; }
    public long RegisterId { get; set; }
    public string RegisterName { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public decimal StartingCash { get; set; }
}

public class CashShiftReceiptDto
{
    public long ShiftId { get; set; }
    public string CashRegisterName { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal StartingCash { get; set; }
    public decimal CashSales { get; set; }
    public decimal FloatIns { get; set; }
    public decimal CashDrops { get; set; }
    public decimal Expenses { get; set; }
    public decimal EndingCashExpected { get; set; }
    public decimal EndingCashActual { get; set; }
    public decimal Discrepancy { get; set; }
    public string? ClosingNotes { get; set; }
    public string FormattedText { get; set; } = string.Empty;
}
