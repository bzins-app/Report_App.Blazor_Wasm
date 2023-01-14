namespace Report_App_WASM.Shared;

public class EmailRecipient
{
    public string? Email { get; init; }
    public bool Bcc { get; set; } = false;
}