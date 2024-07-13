using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

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

        public ExcelDocument(string path)
        {
            ExcelName = Path.GetFileNameWithoutExtension(path);
            Rows = new List<ExcelRow>();
            Headers = new List<string>();
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
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var dataSet = reader.AsDataSet();
                int lineNumber = 1;

                if (hasHeaders && dataSet.Tables.Count > 0)
                {
                    var headerTable = dataSet.Tables[0];
                    Headers = headerTable.Rows[0].ItemArray.Select(cell => cell.ToString()).ToList();
                    lineNumber++;
                }

                foreach (DataTable table in dataSet.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var cells = row.ItemArray.Select(cell => cell.ToString()).ToList();
                        var excelRow = new ExcelRow(lineNumber, cells);
                        AddRow(excelRow);
                        lineNumber++;
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
