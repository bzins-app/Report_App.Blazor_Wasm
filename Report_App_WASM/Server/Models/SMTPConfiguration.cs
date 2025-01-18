namespace Report_App_WASM.Server.Models;

public class SmtpConfiguration : BaseTraceability
{
    private string? _password;
    public int Id { get; set; }
    [Required] [MaxLength(250)] public string? ConfigurationName { get; set; }
    [MaxLength(200)] public string? SmtpUserName { get; set; }

    public string? SmtpPassword
    {
        get => _password;
        set
        {
            if (_password == value)
                _password = value;
            else
                _password = EncryptDecrypt.EncryptString(value!);
        }
    }

    [Required] public string? SmtpHost { get; set; }
    [Required] public int SmtpPort { get; set; }
    public bool SmtpSsl { get; set; }
    [Required] [MaxLength(250)] public string? FromEmail { get; set; }
    [Required] [MaxLength(250)] public string? FromFullName { get; set; }
    public bool IsActivated { get; set; }
}