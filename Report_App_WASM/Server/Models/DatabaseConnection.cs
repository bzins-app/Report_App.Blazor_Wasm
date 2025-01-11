﻿namespace Report_App_WASM.Server.Models;

public class DatabaseConnection : BaseTraceability
{
    private string? _password;
    private string? _databaseTypeName;
    public long DatabaseConnectionId { get; set; }
    [MaxLength(20)] public string ConnectionType { get; set; } = "SQL";
    public TypeDb TypeDb { get; set; }

    [MaxLength(20)]
    public string? TypeDbName
    {
        get => _databaseTypeName;
        set
        {
            _databaseTypeName = value;
            _databaseTypeName = TypeDb.ToString();
        }
    }

    [Required][MaxLength(4000)] public string DbConnectionParameters { get; set; } = "[]";
    [MaxLength(1000)] public string? ConnectionLogin { get; set; }
    [MaxLength(1000)]
    public string? Password
    {
        get => _password;
        set
        {
            if (_password == value)
                _password = value;
            else
                _password = EncryptDecrypt.EncryptString(value);
        }
    }

    public int CommandTimeOut { get; set; } = 300;
    public int CommandFetchSize { get; set; } = 131072;
    public bool UseTableMetaData { get; set; } = false;
    public bool UseTableMetaDataFromAnotherProvider { get; set; } = false;
    public long IdTableMetaData { get; set; }
    public int AdHocQueriesMaxNbrofRowsFetched { get; set; } = 100000;
    public int TaskSchedulerMaxNbrofRowsFetched { get; set; } = 1000000;
    public int DataTransferMaxNbrofRowsFetched { get; set; } = 2000000;
    [MaxLength(4000)] public string RetryPatternParameters { get; set; } = "[]";
    [MaxLength(4000)] public string MiscParameters { get; set; } = "[]";
    public virtual DataProvider? DataProvider { get; set; }
    public virtual ICollection<TableMetadata> TableMetadata { get; set; } = new List<TableMetadata>();
}