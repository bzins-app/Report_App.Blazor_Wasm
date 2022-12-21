namespace Report_App_WASM.Shared
{
    public class EmailRecipient
    {
        public string? Email { get; set; }
        public bool BCC { get; set; } = false;
    }
}
