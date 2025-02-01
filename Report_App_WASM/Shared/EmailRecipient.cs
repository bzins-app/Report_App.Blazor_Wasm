namespace Report_App_WASM.Shared;

public class EmailRecipient
{
    public required string Email { get; set; }
    public bool Bcc { get; set; } = false;
    public bool Cc { get; set; } = false;
}