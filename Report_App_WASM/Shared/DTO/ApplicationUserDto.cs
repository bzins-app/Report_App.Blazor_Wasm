namespace Report_App_WASM.Shared.DTO;

public class ApplicationUserDto : BaseTraceabilityDto, IDto
{
    public virtual Guid Id { get; set; } = default!;
    public virtual string? UserName { get; set; }
    public virtual string? Email { get; set; }
    public virtual bool EmailConfirmed { get; set; }
    public virtual string? PasswordHash { get; set; }
    public virtual string? PhoneNumber { get; set; }
    public virtual bool PhoneNumberConfirmed { get; set; }
    public virtual bool TwoFactorEnabled { get; set; }
    public virtual DateTimeOffset? LockoutEnd { get; set; }
    public virtual bool LockoutEnabled { get; set; }

    public virtual int AccessFailedCount { get; set; }

    //override identity user, add new column
    public bool IsBaseUser { get; set; } = false;
    [MaxLength(250)] public string? UserFirstName { get; set; }
    [MaxLength(250)] public string? UserLastName { get; set; }
    [MaxLength(10)] public string? ApplicationTheme { get; set; }
    [MaxLength(8)] public string Culture { get; set; } = "en";
}