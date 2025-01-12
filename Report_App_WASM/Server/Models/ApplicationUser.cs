namespace Report_App_WASM.Server.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    //override identity user, add new column
    public bool IsBaseUser { get; set; } = false;

    [MaxLength(250)] public string? UserFirstName { get; set; }

    [MaxLength(250)] public string? UserLastName { get; set; }

    [MaxLength(10)] public string? ApplicationTheme { get; set; }

    [MaxLength(8)] public string Culture { get; set; } = "en";

    public DateTime CreateDateTime { get; set; } = DateTime.Now;

    [MaxLength(250)] public string? CreateUser { get; set; }

    public DateTime ModDateTime { get; set; }

    [MaxLength(250)] public string? ModificationUser { get; set; }
}