namespace Report_App_WASM.Server.Models;

public class SystemUniqueKey : IExcludeAuditTrail
{
    public Guid Id { get; set; }
}