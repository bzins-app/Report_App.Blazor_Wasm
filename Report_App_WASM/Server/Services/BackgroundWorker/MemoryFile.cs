namespace Report_App_WASM.Server.Services.BackgroundWorker;

public record MemoryFile
{
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public byte[] Content { get; init; }
}