using System.Text.Json;

namespace Report_App_WASM.Shared.DatabasesConnectionParameters
{
    public enum ApplicationIntent
    {
        ReadWrite,
        ReadOnly
    }

    public abstract class DatabaseParameters
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public int ConnectTimeout { get; set; } = 15;

        public abstract string BuildConnectionString();

        protected string AddParameterIfNotEmpty(string key, string value)
        {
            return !string.IsNullOrEmpty(value) ? $"{key}={value};" : "";
        }

        protected string AddParameterIfNotDefault(string key, bool value, bool defaultValue) 
        {
            return !value.Equals(defaultValue) ? $"{key}={value};" : "";
        }

        protected string AddParameterIfNotDefault<T>(string key, T value, T defaultValue) where T : IEquatable<T>
        {
            return !value.Equals(defaultValue) ? $"{key}={value};" : "";
        }

        protected string AddEnumParameterIfNotDefault<T>(string key, T value, T defaultValue) where T : Enum
        {
            return !EqualityComparer<T>.Default.Equals(value, defaultValue) ? $"{key}={value};" : "";
        }

        public abstract JsonElement SerializeMembersToJson();
    }

    public class SqlServerParameters : DatabaseParameters
    {
        public SqlServerParameters()
        {
            Port = 1433; // Default SQL Server port
        }

        public bool TrustedConnection { get; set; } = false;
        public bool Encrypt { get; set; } = false;
        public bool TrustServerCertificate { get; set; } = false;
        public ApplicationIntent ApplicationIntent { get; set; } = ApplicationIntent.ReadWrite;
        public string ApplicationName { get; set; } = "";
        public int PacketSize { get; set; } = 8000;
        public bool MultipleActiveResultSets { get; set; } = true;
        public bool Pooling { get; set; } = true;
        public int MinPoolSize { get; set; } = 0;
        public int MaxPoolSize { get; set; } = 100;

        public override string BuildConnectionString()
        {
            var parts = new List<string>
            {
                AddParameterIfNotEmpty("Server", Port==1433||Port==0?Server:Server+","+Port),
                AddParameterIfNotEmpty("Database", Database),
                AddParameterIfNotEmpty("User Id", UserId),
                AddParameterIfNotEmpty("Password", Password),
                AddParameterIfNotEmpty("Integrated Security", TrustedConnection?"SSPI":string.Empty),
                AddParameterIfNotDefault("Encrypt", Encrypt, true),
                AddParameterIfNotDefault("TrustServerCertificate", TrustServerCertificate, false),
                AddEnumParameterIfNotDefault("ApplicationIntent", ApplicationIntent, ApplicationIntent.ReadWrite),
                AddParameterIfNotEmpty("ApplicationName", ApplicationName),
                AddParameterIfNotDefault("Packet Size", PacketSize, 8000),
                AddParameterIfNotDefault("MultipleActiveResultSets", MultipleActiveResultSets, false),
                AddParameterIfNotDefault("Pooling", Pooling, true),
                AddParameterIfNotDefault("Connect Timeout", ConnectTimeout, 15)
            };

            if (Pooling)
            {
                parts.Add(AddParameterIfNotDefault("Min Pool Size", MinPoolSize, 0));
                parts.Add(AddParameterIfNotDefault("Max Pool Size", MaxPoolSize, 100));
            }

            return string.Join("", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        public override JsonElement SerializeMembersToJson()
        {
            // Create a shallow copy of the parameters object
            var parametersCopy = (SqlServerParameters)this.MemberwiseClone();

            // Remove sensitive information
            parametersCopy.UserId = null;
            parametersCopy.Password = null;

            return JsonSerializer.SerializeToElement(parametersCopy) ;
        }
    }

    public class MySqlParameters : DatabaseParameters
    {
        public MySqlParameters()
        {
            Port = 3306; // Default MySQL port
        }

        public string Charset { get; set; } = "utf8mb4";
        public bool UseCompression { get; set; } = false;
        public bool UseSSL { get; set; } = false;
        public bool AllowUserVariables { get; set; } = false;
        public bool ConvertZeroDateTime { get; set; } = false;
        public bool PersistSecurityInfo { get; set; } = false;

        public override string BuildConnectionString()
        {
            var parts = new List<string>
            {
                AddParameterIfNotEmpty("Server", Server),
                AddParameterIfNotDefault("Port", Port, 3306),
                AddParameterIfNotEmpty("Database", Database),
                AddParameterIfNotEmpty("Uid", UserId),
                AddParameterIfNotEmpty("Pwd", Password),
                AddParameterIfNotDefault("Charset", Charset, "utf8mb4"),
                AddParameterIfNotDefault("UseCompression", UseCompression, false),
                AddParameterIfNotDefault("SSL Mode", UseSSL?"required":"Preferred", "none"),
                AddParameterIfNotDefault("AllowUserVariables", AllowUserVariables, false),
                AddParameterIfNotDefault("ConvertZeroDateTime", ConvertZeroDateTime, false),
                AddParameterIfNotDefault("PersistSecurityInfo", PersistSecurityInfo, false),
                AddParameterIfNotDefault("Connect Timeout", ConnectTimeout, 100)
            };

            return string.Join("", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        public override JsonElement SerializeMembersToJson()
        {
            // Create a shallow copy of the parameters object
            var parametersCopy = (MySqlParameters)this.MemberwiseClone();

            // Remove sensitive information
            parametersCopy.UserId = null;
            parametersCopy.Password = null;

            return JsonSerializer.SerializeToElement(parametersCopy) ;
        }
    }

    public class PostgreSqlParameters : DatabaseParameters
    {
        public PostgreSqlParameters()
        {
            Port = 5432; // Default PostgreSQL port
        }

        public bool UseSSL { get; set; } = false;
        public string SearchPath { get; set; } = "public";
        public bool Pooling { get; set; } = true;
        public int MinPoolSize { get; set; } = 0;
        public int MaxPoolSize { get; set; } = 100;
        public string ApplicationName { get; set; } = "";

        public override string BuildConnectionString()
        {
            var parts = new List<string>
            {
                AddParameterIfNotEmpty("Host", Server),
                AddParameterIfNotDefault("Port", Port, 5432),
                AddParameterIfNotEmpty("Database", Database),
                AddParameterIfNotEmpty("Username", UserId),
                AddParameterIfNotEmpty("Password", Password),
                AddParameterIfNotDefault("SSL Mode", UseSSL?"require":"prefer", "prefer"),
                AddParameterIfNotDefault("Trust Server Certificate", UseSSL, false),
                AddParameterIfNotDefault("SearchPath", SearchPath, "public"),
                AddParameterIfNotDefault("Pooling", Pooling, true),
                AddParameterIfNotEmpty("Application Name", ApplicationName),
                AddParameterIfNotDefault("Timeout", ConnectTimeout, 15)
            };

            if (Pooling)
            {
                parts.Add(AddParameterIfNotDefault("Minimum Pool Size", MinPoolSize, 0));
                parts.Add(AddParameterIfNotDefault("Maximum Pool Size", MaxPoolSize, 100));
            }

            return string.Join("", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        public override JsonElement SerializeMembersToJson()
        {
            // Create a shallow copy of the parameters object
            var parametersCopy = (PostgreSqlParameters)this.MemberwiseClone();

            // Remove sensitive information
            parametersCopy.UserId = null;
            parametersCopy.Password = null;

            return JsonSerializer.SerializeToElement(parametersCopy) ;
        }
    }

    public class OracleParameters : DatabaseParameters
    {
        public OracleParameters()
        {
            Port = 1521; // Default Oracle port
        }

        public string ServiceName { get; set; }
        public bool DedicatedAdmin { get; set; } = false;
        public bool Pooling { get; set; } = true;
        public int MinPoolSize { get; set; } = 1;
        public int MaxPoolSize { get; set; } = 100;
        public string LoadBalancing { get; set; } = "no";
        public bool UseDbSchema { get; set; }
        public string Schema { get; set; }

        public override string BuildConnectionString()
        {
            var dataSource = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Server})(PORT={Port}))" +
                             $"(CONNECT_DATA=(SERVICE_NAME={ServiceName})" + (DedicatedAdmin ?
                             "(SERVER=DEDICATED)":"") +
                             "))";

            var parts = new List<string>
            {
                AddParameterIfNotEmpty("Data Source", dataSource),
                AddParameterIfNotEmpty("User Id", UserId),
                AddParameterIfNotEmpty("Password", Password),
                AddParameterIfNotDefault("Pooling", Pooling, true),
                AddParameterIfNotDefault("Load Balancing", LoadBalancing, "no"),
                AddParameterIfNotDefault("Connection Timeout", ConnectTimeout, 15)
            };
            if (Pooling)
            {
                parts.Add(AddParameterIfNotDefault("Min Pool Size", MinPoolSize, 1));
                parts.Add(AddParameterIfNotDefault("Max Pool Size", MaxPoolSize, 100));
            }

            return string.Join("", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        public override JsonElement SerializeMembersToJson()
        {
            // Create a shallow copy of the parameters object
            var parametersCopy = (OracleParameters)this.MemberwiseClone();

            // Remove sensitive information
            parametersCopy.UserId = null;
            parametersCopy.Password = null;

            return JsonSerializer.SerializeToElement(parametersCopy) ;
        }
    }

    public class MariaDbParameters : DatabaseParameters
    {
        public MariaDbParameters()
        {
            Port = 3306; // Default MariaDB port (same as MySQL)
        }

        public string Charset { get; set; } = "utf8mb4";
        public bool UseCompression { get; set; } = false;
        public bool UseSSL { get; set; } = false;
        public bool TreatTinyAsBoolean { get; set; } = false;
        public bool AllowUserVariables { get; set; } = false;
        public bool InteractiveSession { get; set; } = false;

        public override string BuildConnectionString()
        {
            var parts = new List<string>
            {
                AddParameterIfNotEmpty("Server", Server),
                AddParameterIfNotDefault("Port", Port, 3306),
                AddParameterIfNotEmpty("Database", Database),
                AddParameterIfNotEmpty("Uid", UserId),
                AddParameterIfNotEmpty("Pwd", Password),
                AddParameterIfNotDefault("Charset", Charset, "utf8mb4"),
                AddParameterIfNotDefault("Use Compression", UseCompression, false),
                AddParameterIfNotDefault("SSL Mode", UseSSL?"required":"Preferred", "none"),
                AddParameterIfNotDefault("TreatTinyAsBoolean", TreatTinyAsBoolean, false),
                AddParameterIfNotDefault("AllowUserVariables", AllowUserVariables, false),
                AddParameterIfNotDefault("InteractiveSession", InteractiveSession, false),
                AddParameterIfNotDefault("Connect Timeout", ConnectTimeout, 100)
            };

            return string.Join("", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        public override JsonElement SerializeMembersToJson()
        {
            // Create a shallow copy of the parameters object
            var parametersCopy = (MariaDbParameters)this.MemberwiseClone();

            // Remove sensitive information
            parametersCopy.UserId = null;
            parametersCopy.Password = null;

            return JsonSerializer.SerializeToElement(parametersCopy) ;
        }
    }

    public class OleDbParameters : DatabaseParameters
    {
        public string Provider { get; set; }
        public string DataSource { get; set; }
        public string ExtendedProperties { get; set; } = "";
        public bool PersistSecurityInfo { get; set; } = false;

        public override string BuildConnectionString()
        {
            var parts = new List<string>
            {
                AddParameterIfNotEmpty("Provider", Provider),
                AddParameterIfNotEmpty("Data Source", DataSource),
                AddParameterIfNotEmpty("Initial Catalog", Database),
                AddParameterIfNotEmpty("User ID", UserId),
                AddParameterIfNotEmpty("Password", Password),
                AddParameterIfNotEmpty("Extended Properties", ExtendedProperties),
                AddParameterIfNotDefault("Persist Security Info", PersistSecurityInfo, false),
                AddParameterIfNotDefault("Connect Timeout", ConnectTimeout, 15)
            };

            return string.Join("", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        public override JsonElement SerializeMembersToJson()
        {
            // Create a shallow copy of the parameters object
            var parametersCopy = (OleDbParameters)this.MemberwiseClone();

            // Remove sensitive information
            parametersCopy.UserId = null;
            parametersCopy.Password = null;

            return JsonSerializer.SerializeToElement(parametersCopy) ;
        }
    }
}
