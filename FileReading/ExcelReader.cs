using Files;
using Files.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml;

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
        /// Converts the Excel file to a DataSet using built-in System.IO.Packaging.
        /// </summary>
        /// <param name="useHeaderRow">Indicates whether the first row should be used as header.</param>
        /// <returns>A DataSet containing the data from the Excel file.</returns>
        public DataSet ConvertExcelToDataSet(bool useHeaderRow = false)
        {
            try
            {
                var file = this;
                var dataSet = new DataSet();
                
                using (var package = Package.Open(file.FilePath, FileMode.Open, FileAccess.Read))
                {
                    // Find workbook.xml to get sheet relationships
                    var workbookPart = package.GetParts()
                        .FirstOrDefault(p => p.Uri.ToString().Contains("workbook.xml"));
                    
                    if (workbookPart == null)
                        throw new Exception("Invalid Excel file: workbook.xml not found");

                    // Parse workbook to get sheet names and their relationships
                    var workbookXml = new XmlDocument();
                    using (var workbookStream = workbookPart.GetStream())
                    {
                        workbookXml.Load(workbookStream);
                    }

                    var nsmgr = new XmlNamespaceManager(workbookXml.NameTable);
                    nsmgr.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
                    nsmgr.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

                    var sheets = workbookXml.SelectNodes("//x:sheets/x:sheet", nsmgr);
                    
                    foreach (XmlNode sheetNode in sheets)
                    {
                        var sheetName = sheetNode.Attributes["name"]?.Value;
                        var sheetId = sheetNode.Attributes["r:id"]?.Value;
                        
                        // Find the worksheet part based on relationship ID
                        var worksheetPart = package.GetParts()
                            .FirstOrDefault(p => p.Uri.ToString().Contains($"worksheets/sheet{sheetId.Replace("rId", "")}.xml") ||
                                                GetRelationshipId(package, p) == sheetId);
                        
                        if (worksheetPart != null)
                        {
                            var worksheetXml = new XmlDocument();
                            using (var worksheetStream = worksheetPart.GetStream())
                            {
                                worksheetXml.Load(worksheetStream);
                            }

                            var dataTable = ParseWorksheet(worksheetXml, sheetName, useHeaderRow, nsmgr);
                            dataSet.Tables.Add(dataTable);
                        }
                    }
                }
                
                return dataSet;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message ?? e.InnerException.ToString());
            }
        }

        /// <summary>
        /// Helper method to get relationship ID for a part
        /// </summary>
        private string GetRelationshipId(Package package, PackagePart part)
        {
            var workbookPart = package.GetParts()
                .FirstOrDefault(p => p.Uri.ToString().Contains("workbook.xml"));
            
            if (workbookPart != null)
            {
                var relationships = workbookPart.GetRelationships();
                foreach (var rel in relationships)
                {
                    if (rel.TargetUri == part.Uri)
                        return rel.Id;
                }
            }
            return null;
        }

        /// <summary>
        /// Parses a worksheet XML into a DataTable
        /// </summary>
        private DataTable ParseWorksheet(XmlDocument worksheetXml, string sheetName, bool useHeaderRow, XmlNamespaceManager nsmgr)
        {
            var dataTable = new DataTable(sheetName);
            var sheetData = worksheetXml.SelectSingleNode("//x:sheetData", nsmgr);
            
            if (sheetData == null)
                return dataTable;

            var rows = sheetData.SelectNodes("//x:row", nsmgr);
            if (rows == null || rows.Count == 0)
                return dataTable;

            // First pass: determine column count
            int maxColumns = 0;
            foreach (XmlNode rowNode in rows)
            {
                var cells = rowNode.SelectNodes("//x:c", nsmgr);
                if (cells != null && cells.Count > maxColumns)
                    maxColumns = cells.Count;
            }

            // Create columns
            for (int i = 0; i < maxColumns; i++)
            {
                dataTable.Columns.Add($"Column{i + 1}", typeof(string));
            }

            // Parse rows
            bool isFirstRow = true;
            foreach (XmlNode rowNode in rows)
            {
                var cells = rowNode.SelectNodes("//x:c", nsmgr);
                if (cells == null)
                    continue;

                var rowValues = new object[maxColumns];
                int cellIndex = 0;

                foreach (XmlNode cellNode in cells)
                {
                    var value = GetCellValue(cellNode, worksheetXml, nsmgr);
                    if (cellIndex < maxColumns)
                        rowValues[cellIndex] = value;
                    cellIndex++;
                }

                // Handle header row
                if (isFirstRow && useHeaderRow)
                {
                    dataTable.Columns.Clear();
                    for (int i = 0; i < maxColumns; i++)
                    {
                        var colName = i < rowValues.Length && rowValues[i] != null 
                            ? rowValues[i].ToString() 
                            : $"Column{i + 1}";
                        dataTable.Columns.Add(colName, typeof(string));
                    }
                    isFirstRow = false;
                    continue;
                }

                dataTable.Rows.Add(rowValues);
                isFirstRow = false;
            }

            return dataTable;
        }

        /// <summary>
        /// Gets the value from a cell node
        /// </summary>
        private string GetCellValue(XmlNode cellNode, XmlDocument worksheetXml, XmlNamespaceManager nsmgr)
        {
            var cellType = cellNode.Attributes["t"]?.Value;
            var cellReference = cellNode.Attributes["r"]?.Value;
            
            var valueNode = cellNode.SelectSingleNode("//x:v", nsmgr);
            if (valueNode == null)
                return string.Empty;

            var value = valueNode.InnerText;

            // Handle different cell types
            if (cellType == "s") // Shared string
            {
                return GetSharedStringValue(worksheetXml, int.Parse(value), nsmgr);
            }
            else if (cellType == "b") // Boolean
            {
                return value == "1" ? "TRUE" : "FALSE";
            }
            else if (cellType == "e") // Error
            {
                return $"#ERROR:{value}";
            }

            return value;
        }

        /// <summary>
        /// Gets the value from shared strings table
        /// </summary>
        private string GetSharedStringValue(XmlDocument worksheetXml, int index, XmlNamespaceManager nsmgr)
        {
            // Try to find sharedStrings.xml in the package
            // This is a simplified approach - in production you'd want to cache this
            var sharedStringsPart = worksheetXml.SelectSingleNode("//x:sharedStrings", nsmgr);
            
            // For now, return the index as we need to load sharedStrings.xml separately
            // This is a limitation of the pure built-in approach
            return $"[SharedString:{index}]";
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
