namespace Report_App_WASM.Server.Models;

public class ActivityDbConnection : BaseTraceability
{
    private string? _password;
    private string? _typeDbName;
    public int Id { get; set; }

    [MaxLength(20)] public string ConnectionType { get; set; } = "SQL";

    public TypeDb TypeDb { get; set; }

    [MaxLength(20)]
    public string? TypeDbName
    {
        get => _typeDbName;
        set
        {
            _typeDbName = value;
            _typeDbName = TypeDb.ToString();
        }
    }

    [Required] public string? ConnectionPath { get; set; }

    public int Port { get; set; }

    [MaxLength(100)] public string? ConnectionLogin { get; set; }

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

    public bool UseDbSchema { get; set; }
    public bool AdAuthentication { get; set; } = false;
    public bool IntentReadOnly { get; set; }

    [MaxLength(100)] public string? DbSchema { get; set; }

    public int CommandTimeOut { get; set; } = 300;
    public int CommandFetchSize { get; set; } = 131072;
    public string DbConnectionParameters { get; set; } = "[]";
    public bool UseTablesDescriptions { get; set; } = false;
    public bool UseDescriptionsFromAnotherActivity { get; set; } = false;
    public int IdDescriptions { get; set; }
    public int AdHocQueriesMaxNbrofRowsFetched { get; set; } = 100000;
    public int TaskSchedulerMaxNbrofRowsFetched { get; set; } = 1000000;
    public int DataTransferMaxNbrofRowsFetched { get; set; } = 2000000;
    public virtual Activity? Activity { get; set; }
    public virtual ICollection<DbTableDescriptions> DbTableDescriptions { get; set; } = new List<DbTableDescriptions>();
}