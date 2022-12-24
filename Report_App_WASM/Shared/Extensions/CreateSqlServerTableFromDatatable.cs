using System.Data;
using System.Text;

namespace Report_App_WASM.Shared.Extensions
{
    public static class CreateSqlServerTableFromDatatable
    {
        public static string CreateTableFromSchema(DataTable dt, string? tableName, bool dropTable, List<string?>? primaryKeys = null)
        {
            // Drop the new table if it is already there.
            StringBuilder sqlCmd = new();
            if (dropTable)
            {
                sqlCmd.Append(
                 "IF OBJECT_ID (N'" + tableName + "', N'U') IS NOT NULL " +
                Environment.NewLine +
                "DROP TABLE " + tableName + ";" + Environment.NewLine +
                Environment.NewLine);
            }
            // Start building a command string to create the table.
            sqlCmd.Append("CREATE TABLE [" + tableName + "] (" +
            Environment.NewLine);
            // Iterate over the column collection in the source table.
            foreach (DataColumn col in dt.Columns)
            {
                // Add the column.
                sqlCmd.Append("[" + col.ColumnName + "] ");
                // Map the source column type to a SQL Server type.
                sqlCmd.Append(NetType2SqlType(col.DataType.ToString(),
                col.MaxLength == -1 ? 2000 : col.MaxLength) + " ");
                // Add identity information.
                if (col.AutoIncrement)
                    sqlCmd.Append("IDENTITY ");
                // Add AllowNull information.
                if (primaryKeys != null)
                {
                    var keyCheck = primaryKeys;
                    keyCheck.ForEach(x => x?.ToLower());
                    sqlCmd.Append((keyCheck.Contains(col.ColumnName, StringComparer.OrdinalIgnoreCase) ? "NOT " : "") + "NULL," +
                    Environment.NewLine);
                }
                else
                {
                    sqlCmd.Append((col.AllowDBNull ? "" : "NOT ") + "NULL," +
                    Environment.NewLine);
                }

            }
            sqlCmd.Remove(sqlCmd.Length - (Environment.NewLine.Length + 1), 1);
            sqlCmd.Append(") ON [PRIMARY];" + Environment.NewLine +
            Environment.NewLine);

            // Add the primary key to the table, if it exists.
            /* if (dt.PrimaryKey != null)
             {
                 sqlCmd.Append("ALTER TABLE " + tableName +
                 " WITH NOCHECK ADD " + Environment.NewLine);
                 sqlCmd.Append("CONSTRAINT [PK_" + tableName +
                 "] PRIMARY KEY CLUSTERED (" + Environment.NewLine);
                 // Add the columns to the primary key.
                 foreach (DataColumn col in dt.PrimaryKey)
                 {
                     sqlCmd.Append("[" + col.ColumnName + "]," +
                     Environment.NewLine);
                 }
                 sqlCmd.Remove(sqlCmd.Length -
                 (Environment.NewLine.Length + 1), 1);
                 sqlCmd.Append(") ON [PRIMARY];" + Environment.NewLine +
                 Environment.NewLine);
             }*/

            if (primaryKeys != null)
            {
                sqlCmd.Append("ALTER TABLE " + tableName +
                " WITH NOCHECK ADD " + Environment.NewLine);
                sqlCmd.Append("CONSTRAINT [PK_" + tableName +
                "] PRIMARY KEY CLUSTERED (" + Environment.NewLine);
                // Add the columns to the primary key.
                foreach (var col in primaryKeys)
                {
                    sqlCmd.Append("[" + col + "]," +
                    Environment.NewLine);
                }
                sqlCmd.Remove(sqlCmd.Length -
                (Environment.NewLine.Length + 1), 1);
                sqlCmd.Append(") ON [PRIMARY];" + Environment.NewLine +
                Environment.NewLine);
            }

            return sqlCmd.ToString();

            static string NetType2SqlType(string netType, int maxLength)
            {
                var sqlType = "";

                // Map the .NET type to the data source type.
                // This is not perfect because mappings are not always one-to-one.
                switch (netType)
                {
                    case "System.Boolean":
                        sqlType = "[bit]";
                        break;
                    case "System.Byte":
                        sqlType = "[tinyint]";
                        break;
                    case "System.Int16":
                        sqlType = "[smallint]";
                        break;
                    case "System.Int32":
                        sqlType = "[int]";
                        break;
                    case "System.Int64":
                        sqlType = "[bigint]";
                        break;
                    case "System.Byte[]":
                        sqlType = "[binary]";
                        break;
                    case "System.Char[]":
                        sqlType = "[nchar] (" + maxLength + ")";
                        break;
                    case "System.String":
                        if (maxLength == 0x3FFFFFFF)
                            sqlType = "[ntext]";
                        if (maxLength > 8000)
                            sqlType = "[nvarchar] (max)";
                        else
                            sqlType = "[nvarchar] (" + maxLength + ")";
                        break;
                    case "System.Single":
                        sqlType = "[real]";
                        break;
                    case "System.Double":
                        sqlType = "[float]";
                        break;
                    case "System.Decimal":
                        sqlType = "[decimal]";
                        break;
                    case "System.DateTime":
                        sqlType = "[datetime]";
                        break;
                    case "System.Guid":
                        sqlType = "[uniqueidentifier]";
                        break;
                    case "System.Object":
                        sqlType = "[sql_variant]";
                        break;
                }

                return sqlType;
            }

        }
    }
}
