using System.Text.Json;

namespace Report_App_WASM.Shared.DatabasesConnectionParameters
{

    public class DatabaseConfig
    {
        public TypeDb Type { get; set; }
        public JsonElement Parameters { get; set; }
    }

    public class DatabaseConnectionParametersManager
    {
        public static string SerializeToJson(DatabaseParameters parameters, TypeDb type)
        {
            // Create a shallow copy of the parameters object
            var parametersCopy = parameters.Clone();

            // Remove sensitive information
            parametersCopy.UserId = null;
            parametersCopy.Password = null;

            var config = new DatabaseConfig
            {
                Type = type,
                Parameters = JsonSerializer.SerializeToElement(parametersCopy)
            };
            return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        }

        public static DatabaseParameters DeserializeFromJson(string json, string userId, string password)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var config = JsonSerializer.Deserialize<DatabaseConfig>(json, options);
            DatabaseParameters parameters = (config.Type switch
            {
                TypeDb.SqlServer => JsonSerializer.Deserialize<SqlServerParameters>(config.Parameters.GetRawText(), options),
                TypeDb.MySql => JsonSerializer.Deserialize<MySqlParameters>(config.Parameters.GetRawText(), options),
                TypeDb.PostgreSql => JsonSerializer.Deserialize<PostgreSqlParameters>(config.Parameters.GetRawText(), options),
                TypeDb.Oracle => JsonSerializer.Deserialize<OracleParameters>(config.Parameters.GetRawText(), options),
                TypeDb.MariaDb => JsonSerializer.Deserialize<MariaDbParameters>(config.Parameters.GetRawText(), options),
                TypeDb.OlebDb => JsonSerializer.Deserialize<OleDbParameters>(config.Parameters.GetRawText(), options),
                TypeDb.Db2 => JsonSerializer.Deserialize<OleDbParameters>(config.Parameters.GetRawText(), options),
                _ => throw new ArgumentException("Unsupported database type")
            })!;

            // Add the credentials back
            parameters.UserId = userId;
            parameters.Password = password;

            return parameters;
        }

        public static string BuildConnectionString(string json, string userId, string password)
        {
            var parameters = DeserializeFromJson(json, userId, password);
            return parameters.BuildConnectionString();
        }
    }
}
