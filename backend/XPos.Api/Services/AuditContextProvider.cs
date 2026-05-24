using Microsoft.AspNetCore.Http;
using System.Linq;
using XPos.Domain.Interfaces;

namespace XPos.Api.Services;

public class AuditContextProvider : IAuditContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditContextProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? GetUserId()
    {
        var context = _httpContextAccessor.HttpContext;
        var idVal = context?.User?.FindFirst("id")?.Value;
        if (long.TryParse(idVal, out var id) && id > 0)
        {
            return id;
        }
        return null;
    }

    public string GetIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "unknown";

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.FirstOrDefault();
            if (!string.IsNullOrEmpty(ip))
            {
                return ip.Split(',')[0].Trim();
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public string GetUserAgent()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "unknown";

        var ua = context.Request.Headers["User-Agent"].ToString();
        return string.IsNullOrEmpty(ua) ? "unknown" : ua;
    }
}
