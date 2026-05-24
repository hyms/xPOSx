namespace XPos.Domain.Interfaces;

public interface IAuditContextProvider
{
    long? GetUserId();
    string GetIpAddress();
    string GetUserAgent();
}
