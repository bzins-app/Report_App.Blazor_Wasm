﻿namespace Report_App_WASM.Server.Models;

public class FileStorageLocation : BaseTraceability
{
    public int FileStorageLocationId { get; set; }
    [Required][MaxLength(150)] public string? ConfigurationName { get; set; }
    [Required][MaxLength(1000)] public string FilePath { get; set; } = ".";
    public bool IsReachable { get; set; }
    public bool TryToCreateFolder { get; set; }
    public bool UseSftpProtocol { get; set; } = false;
    public virtual SftpConfiguration? SftpConfiguration { get; set; }
}