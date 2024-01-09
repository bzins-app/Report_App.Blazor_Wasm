using System.Text;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.ExcelUtilities;
using OfficeOpenXml.Table;

namespace Report_App_WASM.Server.Utils;

public static class CreateFile
{
    public static MemoryFileContainer ExcelFromDatable(string? fileName, ExcelCreationDatatable value)
    {
        using ExcelPackage excel = new();
        excel.Workbook.Properties.Author = "Report Service";
        excel.Workbook.Properties.Title = fileName;
        excel.Workbook.Properties.Subject = fileName;
        excel.Workbook.Properties.Created = DateTime.Now;
        excel.Workbook.Properties.LastModifiedBy = "Report Service";

        var workSheet = excel.Workbook.Worksheets.Add(value.TabName);
        if (value.Data.Rows.Count > 0)
        {
            value.Data.TableName = value.TabName.RemoveSpecialCharacters();
            workSheet.Cells[1, 1].LoadFromDataTable(value.Data, true, TableStyles.Light13);
            var colNumber = 1;

            foreach (DataColumn col in value.Data.Columns)
            {
                if (col.DataType == typeof(DateTime))
                    workSheet.Column(colNumber).Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                colNumber++;
            }

            if (value.Data.Columns.Count < 20 && value.Data.Rows.Count < 50000)
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
            //convert table to range
            // workSheet.Tables.Delete(0);
        }

        excel.Save();

        var file = new MemoryFileContainer
        {
            Content = excel.GetAsByteArray(),
            ContentType = "application/vnd.ms-excel",
            FileName = fileName
        };
        return file;
    }

    public static MemoryFileContainer ExcelTemplateFromSeveralDatable(ExcelCreationData dataExcel, FileInfo file)
    {
        using ExcelPackage excel = new(file);
        excel.Workbook.Properties.Author = "Report Service";
        excel.Workbook.Properties.Title = dataExcel.FileName;
        excel.Workbook.Properties.Subject = dataExcel.FileName;
        excel.Workbook.Properties.Created = DateTime.Now;
        excel.Workbook.Properties.LastModifiedBy = "Report Service";

        foreach (var value in dataExcel.Data!)
        {
            var workSheet = excel.Workbook.Worksheets[value.TabName];
            if (value.Data.Rows.Count > 0)
            {
                if (value.ExcelTemplate.UseAnExcelDataTable)
                {
                    var tableName = value.ExcelTemplate.ExcelDataTableName;
                    var table = workSheet.Tables[tableName];
                    value.Data.TableName = tableName;
                    if (table.Range.Rows > 2) table.DeleteRow(1, table.Range.Rows - 2);
                    if (value.Data.Rows.Count > table.Range.Rows - 1) table.AddRow(value.Data.Rows.Count - 1);
                    table.Range.LoadFromDataTable(value.Data, true);
                    if (value.Data.Columns.Count < 20 && value.Data.Rows.Count < 50000)
                        workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                }
                else
                {
                    var tableName = value.TabName.RemoveSpecialCharacters() + DateTime.Now.ToString("yyyyMMddHHmmss");
                    value.Data.TableName = tableName;
                    workSheet.Cells[value.ExcelTemplate.ExcelTemplateCellReference]
                        .LoadFromDataTable(value.Data, true, TableStyles.Light13);

                    if (value.Data.Columns.Count < 20 && value.Data.Rows.Count < 50000)
                        workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                    //convert table to range
                    workSheet.Tables.Delete(tableName);
                }
                //convert table to range
                // workSheet.Tables.Delete(tableName);
            }
        }

        excel.Save();

        var oFile = new MemoryFileContainer
        {
            Content = excel.GetAsByteArray(),
            ContentType = "application/vnd.ms-excel",
            FileName = dataExcel.FileName
        };
        return oFile;

    }

    public static MemoryFileContainer ExcelFromSeveralsDatable(ExcelCreationData dataExcel)
    {
 
        using ExcelPackage excel = new();
        excel.Workbook.Properties.Author = "Report Service";
        excel.Workbook.Properties.Title = dataExcel.FileName;
        excel.Workbook.Properties.Subject = dataExcel.FileName;
        excel.Workbook.Properties.Created = DateTime.Now;
        excel.Workbook.Properties.LastModifiedBy = "Report Service";

        var workSheetValidation = excel.Workbook.Worksheets.Add("Report information");
        workSheetValidation.Cells["A1"].Value = dataExcel.ValidationText;
        workSheetValidation.Cells["A4"].Value = "Report title";
        workSheetValidation.Cells["B4"].Value = "Created at (UTC Time)";
        workSheetValidation.Cells["C4"].Value = "Created by";
        workSheetValidation.Cells["D4"].Value = "Nbr of lines with header";

        var excelLine = 5;
        foreach (var value in dataExcel.Data!)
        {
            var workSheet = excel.Workbook.Worksheets.Add(value.TabName);
            if (value.Data.Rows.Count > 0)
            {
                value.Data.TableName = ExcelAddressUtil.GetValidName(value.TabName.RemoveSpecialCharacters());

                workSheet.Cells[1, 1].LoadFromDataTable(value.Data, true, TableStyles.Light13);
                var colNumber = 1;

                foreach (DataColumn col in value.Data.Columns)
                {
                    if (col.DataType == typeof(DateTime))
                        workSheet.Column(colNumber).Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                    colNumber++;
                }

                if (value.Data.Columns.Count < 20 && value.Data.Rows.Count < 50000)
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                //convert table to range
                // workSheet.Tables.Delete(0);

                if (dataExcel.ValidationSheet)
                {
                    workSheetValidation.Cells["A" + excelLine].Value = value.TabName;
                    workSheetValidation.Cells["B" + excelLine].Value = DateTime.UtcNow;
                    workSheetValidation.Cells["C" + excelLine].Value = "Report Service";
                    workSheetValidation.Cells["D" + excelLine].Value = value.Data.Rows.Count + 1;
                }

                excelLine++;
            }
        }

        if (dataExcel.ValidationSheet)
        {
            var firstRow = 4;
            var lastRow = workSheetValidation.Dimension.End.Row;
            var firstColumn = 1;
            var lastColumn = workSheetValidation.Dimension.End.Column;
            var rg = workSheetValidation.Cells[firstRow, firstColumn, lastRow, lastColumn];
            var tableName = "TableValidation";
            workSheetValidation.Column(2).Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
            workSheetValidation.Cells["A1:A1"].Style.Font.Size = 13;
            workSheetValidation.Cells["A1:A1"].Style.Font.Name = "Calibri";
            workSheetValidation.Cells["A1:A1"].Style.Font.Bold = true;

            //Ading a table to a Range
            var tab = workSheetValidation.Tables.Add(rg, tableName);

            //Formating the table style
            tab.TableStyle = TableStyles.Light8;

            workSheetValidation.Protection.AllowAutoFilter = true;
            workSheetValidation.Protection.AllowDeleteColumns = false;
            workSheetValidation.Protection.AllowDeleteRows = false;
            workSheetValidation.Protection.AllowEditObject = false;
            workSheetValidation.Protection.AllowEditScenarios = false;
            workSheetValidation.Protection.AllowFormatCells = true;
            workSheetValidation.Protection.AllowFormatColumns = true;
            workSheetValidation.Protection.AllowFormatRows = true;
            workSheetValidation.Protection.AllowInsertColumns = false;
            workSheetValidation.Protection.AllowInsertHyperlinks = false;
            workSheetValidation.Protection.AllowInsertRows = false;
            workSheetValidation.Protection.AllowPivotTables = true;
            workSheetValidation.Protection.AllowSelectLockedCells = true;
            workSheetValidation.Protection.AllowSelectUnlockedCells = true;
            workSheetValidation.Protection.AllowSort = true;
            workSheetValidation.Protection.IsProtected = true;
            workSheetValidation.Protection.SetPassword("hunter2");
            workSheetValidation.Cells[workSheetValidation.Dimension.Address].AutoFitColumns();
        }
        else
        {
            excel.Workbook.Worksheets.Delete(workSheetValidation);
        }

        excel.Save();

        var oFile = new MemoryFileContainer
        {
            Content = excel.GetAsByteArray(),
            ContentType = "application/vnd.ms-excel",
            FileName = dataExcel.FileName
        };
        return oFile;
    }

    public static MemoryFileContainer JsonFromDatable(string fileName, DataTable data, string encoding)
    {
        var strJson = JsonConvert.SerializeObject(data);
        var cleaned = strJson.Replace("\n", "").Replace("\r", "");
        byte[] bytes;
        switch (encoding)
        {
            case "Latin1":
                var toConvert = Encoding.Latin1.GetBytes(cleaned);
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.Latin1, toConvert);
                break;
            case "UTF32":
                var toConvertUtf32 = Encoding.UTF32.GetBytes(cleaned);
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, toConvertUtf32);
                break;
            case "UTF16":
                var toConvertUnicode = Encoding.Unicode.GetBytes(cleaned);
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, toConvertUnicode);
                break;
            case "ASCII":
                var toConvertAscii = Encoding.ASCII.GetBytes(cleaned);
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, toConvertAscii);
                break;
            default:
                bytes = Encoding.UTF8.GetBytes(cleaned);
                break;
        }
        var oFile = new MemoryFileContainer
        {
            Content = bytes,
            ContentType = "application/json",
            FileName = fileName
        };
        return oFile;
    }

    public static MemoryFileContainer CsvFromDatable(string fileName, DataTable data, string? encoding,
        string? delimiter = ";", bool removeHeader = false)
    {
        StringBuilder sb = new();

        if (!removeHeader)
        {
            var columnNames = data.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(delimiter, columnNames));
        }

        foreach (DataRow row in data.Rows)
        {
            var fields
                = row.ItemArray.Select(
                    field => field?.ToString()?.Replace("\n", "").Replace("\r", ""));
            sb.AppendLine(string.Join(delimiter, fields));
        }

        byte[] bytes;
        switch (encoding)
        {
            case "Latin1":
                var toConvert = Encoding.Latin1.GetBytes(sb.ToString());
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.Latin1, toConvert);
                break;
            case "UTF32":
                var toConvertUtf32 = Encoding.UTF32.GetBytes(sb.ToString());
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, toConvertUtf32);
                break;
            case "UTF16":
                var toConvertUnicode = Encoding.Unicode.GetBytes(sb.ToString());
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, toConvertUnicode);
                break;
            case "ASCII":
                var toConvertAscii = Encoding.ASCII.GetBytes(sb.ToString());
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, toConvertAscii);
                break;
            default:
                bytes = Encoding.UTF8.GetBytes(sb.ToString());
                break;
        }

        var oFile = new MemoryFileContainer
        {
            Content = bytes,
            ContentType = "text/csv",
            FileName = fileName
        };
        return oFile;

    }

    public static MemoryFileContainer XmlFromDatable(string? datatableName, string fileName, string? encoding,
        DataTable data)
    {
        data.TableName = datatableName;
        string result;
        using (StringWriter sw = new())
        {
            data.WriteXml(sw);
            result = sw.ToString();
        }

        byte[] bytes;
        switch (encoding)
        {
            case "Latin1":
                var toConvert = Encoding.Latin1.GetBytes(result);
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.Latin1, toConvert);
                break;
            case "UTF32":
                var toConvertUtf32 = Encoding.UTF32.GetBytes(result);
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, toConvertUtf32);
                break;
            case "UTF16":
                var toConvertUnicode = Encoding.Unicode.GetBytes(result);
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, toConvertUnicode);
                break;
            case "ASCII":
                var toConvertAscii = Encoding.ASCII.GetBytes(result);
                bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, toConvertAscii);
                break;
            default:
                bytes = Encoding.UTF8.GetBytes(result);
                break;
        }

        var oFile = new MemoryFileContainer
        {
            Content = bytes,
            ContentType = "application/xml",
            FileName = fileName
        };
        return oFile;
    }

    public static MemoryFileContainer ExcelFromCollection<TEntity>(string fileName, string tabName, List<TEntity> data)
        where TEntity : class
    {
        MemoryStream outputStream = new();
        ExcelPackage excel = new();
        var workSheet = excel.Workbook.Worksheets.Add(tabName);
        workSheet.Cells[1, 1].LoadFromCollection(data, true, TableStyles.Light13);
        if (data.Any())
        {
            var valueType = data[0].GetType().GetProperties().Select(p => new
            {
                data = p.Name,
                type = p.PropertyType.Name.StartsWith("String") ? "text" :
                    p.PropertyType.Name.StartsWith("Date") ? "date" : "numeric"
            }).ToList();

            var colNumber = 1;
            foreach (var t in valueType)
            {
                if (t.type == "date") workSheet.Column(colNumber).Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                colNumber++;
            }

            if (data.Count < 10000)
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
        }

        excel.Save();
        var fName = string.Format(fileName + "-{0}", DateTime.Now.ToString("s") + ".xlsx");
        outputStream.Position = 0;

        var oFile = new MemoryFileContainer
        {
            Content = excel.GetAsByteArray(),
            ContentType = "application/vnd.ms-excel",
            FileName = fName
        };
        excel.Dispose();
        outputStream.Dispose();
        return oFile;

    }
}