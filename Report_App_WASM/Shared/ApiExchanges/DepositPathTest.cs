namespace Report_App_WASM.Shared.ApiExchanges;

public class DepositPathTest
{
    public bool UseSftpProtocol { get; init; }
    public string? FilePath { get; init; }
    public bool TryToCreateFolder { get; init; }
    public int SftpConfigurationId { get; init; }
}