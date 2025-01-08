﻿namespace Report_App_WASM.Shared.DTO;

public class SystemLogDto : IDto
{
    public int Id { get; set; }
    public DateTime TimeStampAppHour { get; set; } = DateTime.Now;
    [MaxLength(1000)] public string? Browser { get; set; }
    [MaxLength(1000)] public string? Platform { get; set; }
    [MaxLength(1000)] public string? FullVersion { get; set; }
    [MaxLength(1000)] public string? Host { get; set; }
    [MaxLength(1000)] public string? Path { get; set; }
    [MaxLength(250)] public string? User { get; set; }
    public int EventId { get; set; }
    public int Level { get; set; }
    public string? Message { get; set; }
    public string? Name { get; set; }
}