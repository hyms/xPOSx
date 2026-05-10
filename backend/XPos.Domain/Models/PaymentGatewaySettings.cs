using System;

namespace XPos.Domain.Models;

public class PaymentGatewaySettings
{
    public long Id { get; set; }
    public string GatewayName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty; // Store encrypted in production
    public string? ApiSecret { get; set; } // Store encrypted in production
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
