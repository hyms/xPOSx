using System;

namespace XPos.Domain.Models;

public class MailSettings
{
    public long Id { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Store encrypted in production
    public string? Encryption { get; set; } // SSL, TLS, etc.
    public string FromAddress { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
