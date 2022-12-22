using System.Data;
using System.Dynamic;
using System.Text;

namespace Report_App_WASM.Shared.Extensions
{
    public static class DataTableExtensions
    {
        public static List<dynamic> ToDynamic(this DataTable dt)
        {
            var dynamicDt = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add(dyn);
                foreach (DataColumn column in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
            }
            return dynamicDt;
        }

        public static List<Dictionary<string, object>> ToDictionnary(this DataTable dt)
        {
            return dt.AsEnumerable().Select(
                row => dt.Columns.Cast<DataColumn>().ToDictionary(
                    column => column.ColumnName,
                    column => row[column]
                )).ToList();
        }

        public static string ToHtml(this DataTable dtInfo)
        {
            StringBuilder tableStr = new();

            if (dtInfo.Rows != null && dtInfo.Rows.Count > 0)
            {
                var columnsQty = dtInfo.Columns.Count;
                var rowsQty = dtInfo.Rows.Count;

                tableStr.Append("<TABLE BORDER=\"1\" cellspacing=\"1\" style=\"font-size:14px\">");
                tableStr.Append("<TR>");
                for (var j = 0; j < columnsQty; j++)
                {
                    tableStr.Append("<TH>" + dtInfo.Columns[j].ColumnName + "</TH>");
                }
                tableStr.Append("</TR>");

                for (var i = 0; i < rowsQty; i++)
                {
                    tableStr.Append("<TR>");
                    for (var k = 0; k < columnsQty; k++)
                    {
                        tableStr.Append("<TD>");
                        tableStr.Append(dtInfo.Rows[i][k].ToString());
                        tableStr.Append("</TD>");
                    }
                    tableStr.Append("</TR>");
                }

                tableStr.Append("</TABLE>");
            }

            return tableStr.ToString();
        }
    }
}
