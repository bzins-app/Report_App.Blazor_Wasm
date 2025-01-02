﻿namespace Report_App_WASM.Server.Models;

    public class ScheduledTask : BaseTraceability
    {
        private string? _typeFileName;
        private string? _typeName;

        public int ScheduledTaskId { get; set; }
        [Required][MaxLength(100)] public string? TaskName { get; set; }
        [Required][MaxLength(60)] public string ProviderName { get; set; }
        public int DataProviderId { get; set; }
        [MaxLength(60)] public string? TaskNamePrefix { get; set; }
        public TaskType Type { get; set; }
        [MaxLength(100)] public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
        [MaxLength(20)]
        public string? TypeName
        {
            get => _typeName;
            set
            {
                _typeName = value;
                _typeName = Type.ToString();
            }
        }

        public FileType TypeFile { get; set; }
        [MaxLength(20)]
        public string? TypeFileName
        {
            get => _typeFileName;
            set
            {
                _typeFileName = value;
                _typeFileName = TypeFile.ToString();
            }
        }

        public bool IsEnabled { get; set; } = false;
        public bool SendByEmail { get; set; } = false;
        public int ReportsRetentionInDays { get; set; } = 90;
        [MaxLength(1000)] public string? Comment { get; set; }
        [MaxLength(1000)] public string Tags { get; set; } = "[]";
        [MaxLength(1000)] public string TaskHeaderParameters { get; set; } = "[]";
        [MaxLength(1000)] public string CronParameters { get; set; } = "[]";
        public bool UseGlobalQueryParameters { get; set; } = false;
        [MaxLength(1000)] public string QueryParameters { get; set; } = "[]";
        public DateTime? LastRunDateTime { get; set; } = null;
        public int FileStorageLocationId { get; set; }
        [MaxLength(1000)] public string MiscParamters { get; set; } = "[]";

        public ICollection<ScheduledTaskQuery> TaskQueries { get; set; } = new List<ScheduledTaskQuery>();
        public ICollection<ScheduledTaskDistributionList> DistributionLists { get; set; } = new List<ScheduledTaskDistributionList>();
        public DataProvider DataProvider { get; set; } = null!;
    }