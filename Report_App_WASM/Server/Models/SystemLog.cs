using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

namespace Report_App_WASM.Server.Models;

public class SystemLog : Log, IExcludeAuditTrail
{
    public SystemLog(IHttpContextAccessor accessor)
    {
        if (accessor.HttpContext != null)
        {
            Browser = accessor.HttpContext.Request.Headers["Sec-CH-UA"];
            Platform = accessor.HttpContext.Request.Headers["User-Agent"];
            FullVersion = accessor.HttpContext.Request.Headers["Sec-CH-UA-Full-Version"];
            User = accessor.HttpContext.User?.Identity?.Name;
            Path = accessor.HttpContext.Request.Path;
            Host = accessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(Host)) Host = accessor.HttpContext.Connection?.RemoteIpAddress?.ToString();
        }
    }

    protected SystemLog()
    {
    }

    public DateTime TimeStampAppHour { get; set; } = DateTime.Now;
    [MaxLength(1000)] public string? Browser { get; set; }
    [MaxLength(1000)] public string? Platform { get; set; }
    [MaxLength(1000)] public string? FullVersion { get; set; }
    [MaxLength(1000)] public string? Host { get; set; }
    [MaxLength(1000)] public string? Path { get; set; }
    [MaxLength(250)] public string? User { get; set; }
}