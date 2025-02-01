using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using Xunit;
using Report_App_WASM.Server.Utils.FIles;

namespace Report_App_WASM.Tests.Utils.Files
{
    public class CreateFileTests
    {
        [Fact]
        public void ExcelFromDatable_CreatesExcelFile()
        {
            // Arrange
            var fileName = "TestFile";
            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");
            var excelCreationDatatable = new ExcelCreationDatatable("TestTab", new ExcelTemplate(), dataTable);

            // Act
            var result = CreateFile.ExcelFromDatable(fileName, excelCreationDatatable);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/vnd.ms-excel", result.ContentType);
            Assert.Equal(fileName, result.FileName);
            Assert.NotEmpty(result.Content);
        }

        [Fact]
        public void ExcelTemplateFromSeveralDatable_CreatesExcelFile()
        {
            // Arrange
            var fileName = "TestFile";
            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");
            var excelCreationDatatable = new ExcelCreationDatatable("TestTab", new ExcelTemplate(), dataTable);
            var excelCreationData = new ExcelCreationData { FileName = fileName, Data = new List<ExcelCreationDatatable> { excelCreationDatatable } };
            var fileInfo = new FileInfo("template.xlsx");

            // Act
            var result = CreateFile.ExcelTemplateFromSeveralDatable(excelCreationData, fileInfo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/vnd.ms-excel", result.ContentType);
            Assert.Equal(fileName, result.FileName);
            Assert.NotEmpty(result.Content);
        }

        [Fact]
        public void ExcelFromSeveralsDatable_CreatesExcelFile()
        {
            // Arrange
            var fileName = "TestFile";
            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");
            var excelCreationDatatable = new ExcelCreationDatatable("TestTab", new ExcelTemplate(), dataTable);
            var excelCreationData = new ExcelCreationData { FileName = fileName, Data = new List<ExcelCreationDatatable> { excelCreationDatatable } };

            // Act
            var result = CreateFile.ExcelFromSeveralsDatable(excelCreationData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/vnd.ms-excel", result.ContentType);
            Assert.Equal(fileName, result.FileName);
            Assert.NotEmpty(result.Content);
        }

        [Fact]
        public void JsonFromDatable_CreatesJsonFile()
        {
            // Arrange
            var fileName = "TestFile.json";
            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");
            var encoding = "UTF8";

            // Act
            var result = CreateFile.JsonFromDatable(fileName, dataTable, encoding);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/json", result.ContentType);
            Assert.Equal(fileName, result.FileName);
            Assert.NotEmpty(result.Content);
        }

        [Fact]
        public void CsvFromDatable_CreatesCsvFile()
        {
            // Arrange
            var fileName = "TestFile.csv";
            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");
            var encoding = "UTF8";
            var delimiter = ";";
            var removeHeader = false;

            // Act
            var result = CreateFile.CsvFromDatable(fileName, dataTable, encoding, delimiter, removeHeader);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("text/csv", result.ContentType);
            Assert.Equal(fileName, result.FileName);
            Assert.NotEmpty(result.Content);
        }

        [Fact]
        public void XmlFromDatable_CreatesXmlFile()
        {
            // Arrange
            var fileName = "TestFile.xml";
            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");
            var encoding = "UTF8";
            var datatableName = "TestTable";

            // Act
            var result = CreateFile.XmlFromDatable(datatableName, fileName, encoding, dataTable);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/xml", result.ContentType);
            Assert.Equal(fileName, result.FileName);
            Assert.NotEmpty(result.Content);
        }

        [Fact]
        public void ExcelFromCollection_CreatesExcelFile()
        {
            // Arrange
            var fileName = "TestFile";
            var tabName = "TestTab";
            var data = new List<TestEntity> { new TestEntity { Property1 = "Value1", Property2 = DateTime.Now } };

            // Act
            var result = CreateFile.ExcelFromCollection(fileName, tabName, data);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/vnd.ms-excel", result.ContentType);
            Assert.Contains(fileName, result.FileName);
            Assert.NotEmpty(result.Content);
        }

        private class TestEntity
        {
            public string Property1 { get; set; }
            public DateTime Property2 { get; set; }
        }
    }
}
