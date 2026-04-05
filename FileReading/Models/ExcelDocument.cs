using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml;

namespace Files.Models
{
    public class ExcelRow
    {
        public int LineNumber { get; set; }
        public List<string> Cells { get; set; }

        public ExcelRow(int lineNumber, List<string> cells)
        {
            LineNumber = lineNumber;
            Cells = cells;
        }

        public override string ToString()
        {
            return $"Line {LineNumber}: {string.Join(", ", Cells)}";
        }
    }

    public class ExcelDocument
    {
        public List<ExcelRow> Rows { get; private set; }
        public List<string> Headers { get; private set; }
        public string ExcelName { get; private set; }
        public Dictionary<string, List<List<string>>> Sheets { get; private set; }

        public ExcelDocument(string path)
        {
            ExcelName = Path.GetFileNameWithoutExtension(path);
            Rows = new List<ExcelRow>();
            Headers = new List<string>();
            Sheets = new Dictionary<string, List<List<string>>>();
        }

        public void AddRow(ExcelRow row)
        {
            try
            {
                row.LineNumber = Rows.Count + 1;
                Rows.Add(row);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding row: {ex.Message}", ex);
            }
        }

        public void AddRows(IEnumerable<ExcelRow> rows)
        {
            try
            {
                foreach (var row in rows)
                {
                    AddRow(row);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding rows: {ex.Message}", ex);
            }
        }

        public void RemoveRow(int lineNumber)
        {
            try
            {
                var row = GetRow(lineNumber) ?? throw new Exception($"Row with line number {lineNumber} does not exist.");
                Rows.Remove(row);
                ReIndexRows();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing row: {ex.Message}", ex);
            }
        }

        public void RemoveRows(IEnumerable<int> lineNumbers)
        {
            try
            {
                foreach (var lineNumber in lineNumbers)
                {
                    RemoveRow(lineNumber);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing rows: {ex.Message}", ex);
            }
        }

        public ExcelRow GetRow(int lineNumber)
        {
            try
            {
                var row = Rows.FirstOrDefault(r => r.LineNumber == lineNumber);
                return row ?? throw new Exception($"Row with line number {lineNumber} not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting row: {ex.Message}", ex);
            }
        }

        public void UpdateCell(int lineNumber, int cellIndex, string newValue)
        {
            try
            {
                var row = GetRow(lineNumber) ?? throw new Exception($"Row with line number {lineNumber} not found.");
                if (cellIndex < 0 || cellIndex >= row.Cells.Count)
                    throw new Exception($"Cell index {cellIndex} is out of bounds.");

                row.Cells[cellIndex] = newValue;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating cell: {ex.Message}", ex);
            }
        }

        public void UpdateCells(IEnumerable<(int lineNumber, int cellIndex, string newValue)> updates)
        {
            try
            {
                foreach (var (lineNumber, cellIndex, newValue) in updates)
                {
                    UpdateCell(lineNumber, cellIndex, newValue);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating cells: {ex.Message}", ex);
            }
        }

        public List<ExcelRow> FindRowsByCellValue(int cellIndex, string value)
        {
            try
            {
                if (cellIndex < 0)
                    throw new Exception("Cell index cannot be negative.");

                var matchingRows = Rows.Where(r => r.Cells.Count > cellIndex && r.Cells[cellIndex] == value).ToList();
                if (!matchingRows.Any())
                    throw new Exception($"No rows found with cell value '{value}' at index {cellIndex}.");

                return matchingRows;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding rows: {ex.Message}", ex);
            }
        }

        public void SetHeaders(List<string> headers)
        {
            try
            {
                Headers = headers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error setting headers: {ex.Message}", ex);
            }
        }

        public void LoadFromExcel(string filePath, bool hasHeaders = false)
        {
            try
            {
                using var package = Package.Open(filePath, FileMode.Open, FileAccess.Read);
                
                // Find workbook.xml
                var workbookPart = package.GetParts()
                    .FirstOrDefault(p => p.Uri.ToString().Contains("workbook.xml"));
                
                if (workbookPart == null)
                    throw new Exception("Invalid Excel file: workbook.xml not found");

                var workbookXml = new XmlDocument();
                using (var workbookStream = workbookPart.GetStream())
                {
                    workbookXml.Load(workbookStream);
                }

                var nsmgr = new XmlNamespaceManager(workbookXml.NameTable);
                nsmgr.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
                nsmgr.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

                var sheets = workbookXml.SelectNodes("//x:sheets/x:sheet", nsmgr);
                int lineNumber = 1;

                foreach (XmlNode sheetNode in sheets)
                {
                    var sheetName = sheetNode.Attributes["name"]?.Value;
                    var sheetId = sheetNode.Attributes["r:id"]?.Value;
                    
                    // Find worksheet part
                    var worksheetPart = package.GetParts()
                        .FirstOrDefault(p => GetRelationshipId(package, p) == sheetId);
                    
                    if (worksheetPart != null)
                    {
                        var worksheetXml = new XmlDocument();
                        using (var worksheetStream = worksheetPart.GetStream())
                        {
                            worksheetXml.Load(worksheetStream);
                        }

                        var sheetData = ParseWorksheet(worksheetXml, nsmgr, hasHeaders && lineNumber == 1);
                        
                        if (hasHeaders && lineNumber == 1 && sheetData.Any())
                        {
                            Headers = sheetData.First();
                            lineNumber++;
                        }

                        foreach (var row in sheetData)
                        {
                            var excelRow = new ExcelRow(lineNumber, row);
                            AddRow(excelRow);
                            lineNumber++;
                        }

                        Sheets[sheetName] = sheetData;
                    }
                }

                ExcelName = Path.GetFileNameWithoutExtension(filePath);
            }
            catch (FileNotFoundException ex)
            {
                throw new Exception($"File not found: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new Exception($"IO error while reading file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading Excel: {ex.Message}", ex);
            }
        }

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

        private List<List<string>> ParseWorksheet(XmlDocument worksheetXml, XmlNamespaceManager nsmgr, bool skipFirstRow = false)
        {
            var result = new List<List<string>>();
            var sheetData = worksheetXml.SelectSingleNode("//x:sheetData", nsmgr);
            
            if (sheetData == null)
                return result;

            var rows = sheetData.SelectNodes("//x:row", nsmgr);
            if (rows == null || rows.Count == 0)
                return result;

            bool isFirstRow = skipFirstRow;
            foreach (XmlNode rowNode in rows)
            {
                if (isFirstRow)
                {
                    isFirstRow = false;
                    continue;
                }

                var cells = rowNode.SelectNodes("//x:c", nsmgr);
                if (cells == null)
                    continue;

                var rowValues = new List<string>();
                foreach (XmlNode cellNode in cells)
                {
                    var value = GetCellValue(cellNode, worksheetXml, nsmgr);
                    rowValues.Add(value);
                }

                result.Add(rowValues);
            }

            return result;
        }

        private string GetCellValue(XmlNode cellNode, XmlDocument worksheetXml, XmlNamespaceManager nsmgr)
        {
            var cellType = cellNode.Attributes["t"]?.Value;
            
            var valueNode = cellNode.SelectSingleNode("//x:v", nsmgr);
            if (valueNode == null)
                return string.Empty;

            var value = valueNode.InnerText;

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

        private string GetSharedStringValue(XmlDocument worksheetXml, int index, XmlNamespaceManager nsmgr)
        {
            return $"[SharedString:{index}]";
        }

        public void SaveToExcel(string filePath)
        {
            throw new NotImplementedException("Saving to Excel is not implemented in this example.");
        }

        private void ReIndexRows()
        {
            try
            {
                for (int i = 0; i < Rows.Count; i++)
                {
                    Rows[i].LineNumber = i + 1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error re-indexing rows: {ex.Message}", ex);
            }
        }

        public override string ToString()
        {
            try
            {
                return $"Document with {Rows.Count} rows";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting document to string: {ex.Message}", ex);
            }
        }
    }
}
