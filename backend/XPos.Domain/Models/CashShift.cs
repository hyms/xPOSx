using System;

namespace XPos.Domain.Models;

public class CashShift
{
    public long Id { get; set; }
    public long CashRegisterId { get; set; }
    public long UserId { get; set; }
    public string Status { get; set; } = "OPEN"; // "OPEN", "CLOSED"
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal StartingCash { get; set; }
    public decimal EndingCashExpected { get; set; }
    public decimal? EndingCashActual { get; set; }
    public decimal? Discrepancy { get; set; }
    public string? ClosingNotes { get; set; }
}
