namespace Report_App_WASM.Shared.DTO;

public class LdapConfigurationDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }

    [Required] [MaxLength(60)] public string? ConfigurationName { get; set; }

    [Required] public string? Domain { get; set; }

    [MaxLength(100)] public string? UserName { get; set; }

    public string? Password { get; set; }
    public bool IsActivated { get; set; }
}