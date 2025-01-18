namespace Report_App_WASM.Server.Utils.FIles;

public record MemoryFileContainer
{
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public byte[] Content { get; init; }
}