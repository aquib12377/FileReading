using ExcelDataReader;
using Files;
using Files.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Files
{
    public class ExcelReader : FileReading
    {
        public ExcelReader(string filePath, FileType fileType, string seperator = "") : base(filePath, fileType, seperator)
        {
        }

        public ExcelReader()
        {
        }

        /// <summary>
        /// Converts the Excel file to a DataTableCollection.
        /// </summary>
        /// <param name="useHeaderRow">Indicates whether the first row should be used as header.</param>
        /// <returns>A collection of DataTables.</returns>
        public DataTableCollection ConvertExcelToDataTable(bool useHeaderRow = false)
        {
            var file = this;
            if (!ValidateFileData(file)) throw new Exception("Invalid File Data");

            switch (file.FileType)
            {
                case FileType.CSV:
                    throw new Exception("This method cannot get data from CSV File");
                case FileType.TEXT:
                    throw new Exception("This method cannot get data from Text File");
            }

            try
            {
                return ConvertExcelToDataSet(useHeaderRow).Tables;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message ?? e.InnerException.ToString());
            }
        }

        /// <summary>
        /// Converts the Excel file to a DataSet.
        /// </summary>
        /// <param name="useHeaderRow">Indicates whether the first row should be used as header.</param>
        /// <returns>A DataSet containing the data from the Excel file.</returns>
        public DataSet ConvertExcelToDataSet(bool useHeaderRow = false)
        {
            try
            {
                var file = this;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using var stream = File.Open(file.FilePath, FileMode.Open, FileAccess.Read);
                IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = useHeaderRow
                    }
                };
                return reader.AsDataSet(conf);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message ?? e.InnerException.ToString());
            }
        }

        /// <summary>
        /// Converts the Excel file to a list of lists of lists of strings.
        /// </summary>
        /// <returns>A list of lists of lists of strings containing the data from the Excel file.</returns>
        public List<List<List<string>>> ConvertExcelToList()
        {
            var file = this;
            var tables = new List<List<List<string>>>();
            var dataTables = ConvertExcelToDataTable();

            foreach (DataTable table in dataTables)
            {
                var columns = new List<List<string>>();
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    columns.Add(table.Rows.OfType<DataRow>().Select(a => a.ItemArray[i].ToString()).ToList());
                }

                tables.Add(columns);
            }

            return tables;
        }

        /// <summary>
        /// Gets the list of sheet names in the Excel file.
        /// </summary>
        /// <returns>A list of sheet names.</returns>
        public List<string> GetSheetNames()
        {
            var dataSet = ConvertExcelToDataSet();
            return dataSet.Tables.Cast<DataTable>().Select(table => table.TableName).ToList();
        }

        /// <summary>
        /// Extracts a specific sheet from the Excel file as a DataTable.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to extract.</param>
        /// <param name="useHeaderRow">Indicates whether the first row should be used as header.</param>
        /// <returns>The specified sheet as a DataTable.</returns>
        public DataTable ExtractSheet(string sheetName, bool useHeaderRow = false)
        {
            var dataSet = ConvertExcelToDataSet(useHeaderRow);
            return dataSet.Tables[sheetName];
        }

        /// <summary>
        /// Extracts a specific sheet from the Excel file as a list of lists of strings.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to extract.</param>
        /// <returns>The specified sheet as a list of lists of strings.</returns>
        public List<List<string>> ExtractSheetAsList(string sheetName)
        {
            var table = ExtractSheet(sheetName);
            var columns = new List<List<string>>();

            for (var i = 0; i < table.Columns.Count; i++)
            {
                columns.Add(table.Rows.OfType<DataRow>().Select(a => a.ItemArray[i].ToString()).ToList());
            }

            return columns;
        }

        /// <summary>
        /// Checks if the specified sheet exists in the Excel file.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to check for.</param>
        /// <returns>True if the sheet exists, otherwise false.</returns>
        public bool SheetExists(string sheetName)
        {
            var sheetNames = GetSheetNames();
            return sheetNames.Contains(sheetName);
        }

        public ExcelDocument ReadExcel()
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var document = new ExcelDocument(FilePath);

            document.LoadFromExcel(FilePath);

            return document;
        }
    }
}
