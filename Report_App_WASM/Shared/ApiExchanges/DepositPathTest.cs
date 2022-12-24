namespace Report_App_WASM.Shared.ApiExchanges
{
    public class DepositPathTest
    {
        public bool UseSftpProtocol { get; set; }
        public string? FilePath { get; set; }
        public bool TryToCreateFolder { get; set; }
        public int SftpConfigurationId { get; set; }
    }
}
