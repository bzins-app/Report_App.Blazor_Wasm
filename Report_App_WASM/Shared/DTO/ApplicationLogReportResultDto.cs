namespace Report_App_WASM.Shared.DTO;

public class ApplicationLogReportResultDto : IDto
{
    private double _fileSizeInMb;
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }

    [MaxLength(100)] public string? CreatedBy { get; set; }

    public int ActivityId { get; set; }

    [MaxLength(60)] public string? ActivityName { get; set; }

    public int TaskHeaderId { get; set; }

    [MaxLength(200)] public string? ReportName { get; set; }

    [MaxLength(200)] public string? SubName { get; set; }

    [MaxLength(60)] public string? FileType { get; set; }

    [MaxLength(100)] public string? FileName { get; set; }

    public string? ReportPath { get; set; }

    public double FileSizeInMb
    {
        get => _fileSizeInMb;
        set => _fileSizeInMb = Math.Round(value, 2);
    }

    public bool IsAvailable { get; set; } = true;
    public string? Result { get; set; }
    public bool Error { get; set; }
}