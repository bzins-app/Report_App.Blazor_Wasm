using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.ExcelUtilities;
using OfficeOpenXml.Table;
using Report_App_WASM.Shared.Extensions;
using System.Data;
using System.Text;

namespace Report_App_WASM.Server.Utils
{
    public static class CreateFile
    {
        public static FileContentResult ExcelFromDatable(string FileName, ExcelCreationDatatable value)
        {
            MemoryStream outputStream = new();
            using ExcelPackage excel = new(outputStream);
            excel.Workbook.Properties.Author = "Report Service";
            excel.Workbook.Properties.Title = FileName;
            excel.Workbook.Properties.Subject = FileName;
            excel.Workbook.Properties.Created = DateTime.Now;
            excel.Workbook.Properties.LastModifiedBy = "Report Service";

            ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add(value.TabName);
            if (value.Data.Rows.Count > 0)
            {
                value.Data.TableName = value.TabName.RemoveSpecialCharacters();
                workSheet.Cells[1, 1].LoadFromDataTable(value.Data, true, TableStyles.Light13);
                int colNumber = 1;

                foreach (DataColumn col in value.Data.Columns)
                {
                    if (col.DataType == typeof(DateTime))
                    {
                        workSheet.Column(colNumber).Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                    }
                    colNumber++;
                }
                if (value.Data.Columns.Count < 20 && value.Data.Rows.Count < 50000)
                {
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                }
                //convert table to range
                // workSheet.Tables.Delete(0);
            }
            excel.Save();
            outputStream.Position = 0;

            return new FileContentResult(excel.GetAsByteArray(), "application/vnd.ms-excel")
            {
                FileDownloadName = FileName
            };
        }
        public static FileContentResult ExcelTemplateFromSeveralDatable(ExcelCreationData dataExcel, FileInfo file)
        {
            MemoryStream outputStream = new();
            using ExcelPackage excel = new(file);
            excel.Workbook.Properties.Author = "Report Service";
            excel.Workbook.Properties.Title = dataExcel.FileName;
            excel.Workbook.Properties.Subject = dataExcel.FileName;
            excel.Workbook.Properties.Created = DateTime.Now;
            excel.Workbook.Properties.LastModifiedBy = "Report Service";

            foreach (var value in dataExcel.Data)
            {
                ExcelWorksheet workSheet = excel.Workbook.Worksheets[value.TabName];
                if (value.Data.Rows.Count > 0)
                {
                    if (value.ExcelTemplate.UseAnExcelDataTable)
                    {
                        var tableName = value.ExcelTemplate.ExcelDataTableName;
                        var table = workSheet.Tables[tableName];
                        value.Data.TableName = tableName;
                        if (table.Range.Rows > 2)
                        {
                            table.DeleteRow(1, table.Range.Rows - 2);
                        }
                        if (value.Data.Rows.Count > table.Range.Rows - 1)
                        {
                            table.AddRow(value.Data.Rows.Count - 1);
                        }
                        table.Range.LoadFromDataTable(value.Data, true);
                        if (value.Data.Columns.Count < 20 && value.Data.Rows.Count < 50000)
                        {
                            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                        }
                    }
                    else
                    {
                        var tableName = value.TabName.RemoveSpecialCharacters() + DateTime.Now.ToString("yyyyMMddHHmmss");
                        value.Data.TableName = tableName;
                        workSheet.Cells[value.ExcelTemplate.ExcelTemplateCellReference].LoadFromDataTable(value.Data, true, TableStyles.Light13);

                        if (value.Data.Columns.Count < 20 && value.Data.Rows.Count < 50000)
                        {
                            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                        }
                        //convert table to range
                        workSheet.Tables.Delete(tableName);
                    }
                    //convert table to range
                    // workSheet.Tables.Delete(tableName);
                }
            }
            excel.SaveAs(outputStream);
            outputStream.Position = 0;

            return new FileContentResult(excel.GetAsByteArray(), "application/vnd.ms-excel")
            {
                FileDownloadName = dataExcel.FileName
            };
        }

        public static FileContentResult ExcelFromSeveralsDatable(ExcelCreationData dataExcel)
        {
            MemoryStream outputStream = new();
            using ExcelPackage excel = new(outputStream);
            excel.Workbook.Properties.Author = "Report Service";
            excel.Workbook.Properties.Title = dataExcel.FileName;
            excel.Workbook.Properties.Subject = dataExcel.FileName;
            excel.Workbook.Properties.Created = DateTime.Now;
            excel.Workbook.Properties.LastModifiedBy = "Report Service";

            ExcelWorksheet workSheetValidation = excel.Workbook.Worksheets.Add("Report information");
            workSheetValidation.Cells["A1"].Value = dataExcel.ValidationText;
            workSheetValidation.Cells["A4"].Value = "Report title";
            workSheetValidation.Cells["B4"].Value = "Created at (UTC Time)";
            workSheetValidation.Cells["C4"].Value = "Created by";
            workSheetValidation.Cells["D4"].Value = "Nbr of lines with header";

            int excelLine = 5;
            foreach (var value in dataExcel.Data)
            {
                ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add(value.TabName);
                if (value.Data.Rows.Count > 0)
                {
                    value.Data.TableName = ExcelAddressUtil.GetValidName(value.TabName.RemoveSpecialCharacters());

                    workSheet.Cells[1, 1].LoadFromDataTable(value.Data, true, TableStyles.Light13);
                    int colNumber = 1;

                    foreach (DataColumn col in value.Data.Columns)
                    {
                        if (col.DataType == typeof(DateTime))
                        {
                            workSheet.Column(colNumber).Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                        }
                        colNumber++;
                    }
                    if (value.Data.Columns.Count < 20 && value.Data.Rows.Count < 50000)
                    {
                        workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                    }
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
                int firstRow = 4;
                int lastRow = workSheetValidation.Dimension.End.Row;
                int firstColumn = 1;
                int lastColumn = workSheetValidation.Dimension.End.Column;
                ExcelRange rg = workSheetValidation.Cells[firstRow, firstColumn, lastRow, lastColumn];
                string tableName = "TableValidation";
                workSheetValidation.Column(2).Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                workSheetValidation.Cells["A1:A1"].Style.Font.Size = 13;
                workSheetValidation.Cells["A1:A1"].Style.Font.Name = "Calibri";
                workSheetValidation.Cells["A1:A1"].Style.Font.Bold = true;

                //Ading a table to a Range
                ExcelTable tab = workSheetValidation.Tables.Add(rg, tableName);

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
            outputStream.Position = 0;

            return new FileContentResult(excel.GetAsByteArray(), "application/vnd.ms-excel")
            {
                FileDownloadName = dataExcel.FileName
            };
        }

        public static FileContentResult JsonFromDatable(string FileName, DataTable data, string encoding)
        {
            var strJson = JsonConvert.SerializeObject(data);
            string cleaned = strJson.Replace("\n", "").Replace("\r", "");
            byte[] bytes;
            switch (encoding)
            {
                case "Latin1":
                    var toConvert = Encoding.Latin1.GetBytes(cleaned);
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.Latin1, toConvert);
                    break;
                case "UTF32":
                    var toConvertUTF32 = Encoding.UTF32.GetBytes(cleaned);
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, toConvertUTF32);
                    break;
                case "UTF16":
                    var toConvertUnicode = Encoding.Unicode.GetBytes(cleaned);
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, toConvertUnicode);
                    break;
                case "ASCII":
                    var toConvertASCII = Encoding.ASCII.GetBytes(cleaned);
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, toConvertASCII);
                    break;
                default:
                    bytes = Encoding.UTF8.GetBytes(cleaned);
                    break;
            }
            return new FileContentResult(bytes, "application/json")
            {
                FileDownloadName = FileName
            };
        }

        public static FileContentResult CsvFromDatable(string FileName, DataTable data, string encoding, string delimiter = ";", bool removeHeader = false)
        {
            string delimitervalue = delimiter;

            StringBuilder sb = new();

            if (!removeHeader)
            {
                IEnumerable<string> columnNames = data.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                sb.AppendLine(string.Join(delimitervalue, columnNames));
            }

            foreach (DataRow row in data.Rows)
            {
                IEnumerable<string> fields
                    = row.ItemArray.Select(
                    field => field.ToString().Replace("\n", "").Replace("\r", ""));
                sb.AppendLine(string.Join(delimitervalue, fields));
            }

            byte[] bytes;
            switch (encoding)
            {
                case "Latin1":
                    var toConvert = Encoding.Latin1.GetBytes(sb.ToString());
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.Latin1, toConvert);
                    break;
                case "UTF32":
                    var toConvertUTF32 = Encoding.UTF32.GetBytes(sb.ToString());
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, toConvertUTF32);
                    break;
                case "UTF16":
                    var toConvertUnicode = Encoding.Unicode.GetBytes(sb.ToString());
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, toConvertUnicode);
                    break;
                case "ASCII":
                    var toConvertASCII = Encoding.ASCII.GetBytes(sb.ToString());
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, toConvertASCII);
                    break;
                default:
                    bytes = Encoding.UTF8.GetBytes(sb.ToString());
                    break;
            }

            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = FileName
            };
        }

        public static FileContentResult XMLFromDatable(string datatableName, string FileName, string encoding, DataTable data)
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
                    var toConvertUTF32 = Encoding.UTF32.GetBytes(result);
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, toConvertUTF32);
                    break;
                case "UTF16":
                    var toConvertUnicode = Encoding.Unicode.GetBytes(result);
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, toConvertUnicode);
                    break;
                case "ASCII":
                    var toConvertASCII = Encoding.ASCII.GetBytes(result);
                    bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, toConvertASCII);
                    break;
                default:
                    bytes = Encoding.UTF8.GetBytes(result);
                    break;
            }
            return new FileContentResult(bytes, "application/xml")
            {
                FileDownloadName = FileName
            };
        }

        public static FileContentResult ExcelFromCollection<TEntity>(string FileName, string TabName, List<TEntity> Data) where TEntity : class
        {
            MemoryStream outputStream = new();
            ExcelPackage excel = new();
            ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add(TabName);
            workSheet.Cells[1, 1].LoadFromCollection(Data, true, TableStyles.Light13);
            if (Data.Any())
            {
                var ValueType = Data[0].GetType().GetProperties().Select(p => new { data = p.Name, type = p.PropertyType.Name.StartsWith("String") ? "text" : p.PropertyType.Name.StartsWith("Date") ? "date" : "numeric" }).ToList();

                int colNumber = 1;
                foreach (var t in ValueType)
                {
                    if (t.type == "date")
                    {
                        workSheet.Column(colNumber).Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                    }
                    colNumber++;
                }

                if (Data.Count < 10000)
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
            }
            excel.Save();
            string fName = string.Format(FileName + "-{0}", DateTime.Now.ToString("s") + ".xlsx");
            outputStream.Position = 0;

            return new FileContentResult(excel.GetAsByteArray(), "application/vnd.ms-excel")
            {
                FileDownloadName = fName
            };
        }
    }
}
