namespace XPos.Domain.Models;

public class Setting
{
    public long Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyAddress { get; set; }
    public string? Logo { get; set; }
    public string? Favicon { get; set; }
    public string Version { get; set; } = "1.0.0";
    public int SettingsVersion { get; set; } = 1;
    public int Days { get; set; }

    // Currency settings
    public string? CurrencyCode { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? CurrencyName { get; set; }

    // Mail settings
    public string? MailHost { get; set; }
    public int? MailPort { get; set; }
    public string? MailUsername { get; set; }
    public string? MailPassword { get; set; }
    public string? MailEncryption { get; set; }
    public string? MailFromAddress { get; set; }
    public string? MailFromName { get; set; }

    // SMS settings
    public string? SmsGatewayName { get; set; }
    public string? SmsApiKey { get; set; }
    public string? SmsApiSecret { get; set; }
    public string? SmsSenderId { get; set; }
    public bool SmsIsActive { get; set; }
    public string? SmsDescription { get; set; }

    // Payment Gateway settings
    public string? PaymentGatewayName { get; set; }
    public string? PaymentApiKey { get; set; }
    public string? PaymentApiSecret { get; set; }
    public bool PaymentIsActive { get; set; }
    public string? PaymentDescription { get; set; }

    // SIAT Bolivia
    public string? SiatToken { get; set; }
    public string? SiatCertificate { get; set; }
    public int? SiatEnvironment { get; set; }
    public int? SiatModality { get; set; }
    public int? SiatEmissionType { get; set; }
    public string? QrCodePath { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
