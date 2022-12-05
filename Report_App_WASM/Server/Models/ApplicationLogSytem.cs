using Microsoft.AspNetCore.Http;
using Report_App_WASM.Server.Models.AuditModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

namespace Report_App_WASM.Server.Models
{
    public class ApplicationLogSystem : Log, IExcludeAuditTrail
    {
        public ApplicationLogSystem(IHttpContextAccessor accessor)
        {
            if (accessor.HttpContext != null)
            {
                Browser = accessor.HttpContext.Request.Headers["Sec-CH-UA"];
                Platform = accessor.HttpContext.Request.Headers["User-Agent"];
                FullVersion = accessor.HttpContext.Request.Headers["Sec-CH-UA-Full-Version"];
                User = accessor.HttpContext.User?.Identity?.Name;
                Path = accessor.HttpContext.Request.Path;

                Host = accessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(Host))
                {
                    Host = accessor.HttpContext.Connection?.RemoteIpAddress?.ToString();
                }
            }
        }

        protected ApplicationLogSystem()
        {
        }
        public DateTime TimeStampAppHour { get; set; } = DateTime.Now;
        [MaxLength(600)]
        public string? Browser { get; set; }
        [MaxLength(600)]
        public string? Platform { get; set; } 
        [MaxLength(600)]
        public string? FullVersion { get; set; }
        [MaxLength(600)]
        public string? Host { get; set; } 
        [MaxLength(600)]
        public string? Path { get; set; } 
        [MaxLength(200)]
        public string? User { get; set; } 

    }
}
