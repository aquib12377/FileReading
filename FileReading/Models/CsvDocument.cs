using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Files.Models
{
    public class CSVRow
    {
        public int LineNumber { get; set; }
        public List<string> Cells { get; set; }

        public CSVRow(int lineNumber, List<string> cells)
        {
            LineNumber = lineNumber;
            Cells = cells;
        }

        public override string ToString()
        {
            return $"Line {LineNumber}: {string.Join(", ", Cells)}";
        }
    }

    public class CsvDocument
    {
        public List<CSVRow> Rows { get; private set; }
        public List<string> Headers { get; private set; }
        public string CsvName { get; private set; }
        public char Separator { get; private set; }

        public CsvDocument(string path,char separator = ',')
        {
            CsvName = Path.GetFileNameWithoutExtension(path);
            Rows = new List<CSVRow>();
            Headers = new List<string>();
            Separator = separator;
        }

        public void AddRow(CSVRow row)
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

        public void AddRows(IEnumerable<CSVRow> rows)
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

        public CSVRow GetRow(int lineNumber)
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

        public List<CSVRow> FindRowsByCellValue(int cellIndex, string value)
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

        public void LoadFromCsv(string filePath, bool hasHeaders = false)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    int lineNumber = 1;
                    if (hasHeaders)
                    {
                        var headerLine = reader.ReadLine();
                        if (headerLine != null)
                        {
                            Headers = ParseCsvLine(headerLine);
                        }
                        lineNumber++;
                    }

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            var cells = ParseCsvLine(line);
                            var row = new CSVRow(lineNumber, cells);
                            AddRow(row);
                            lineNumber++;
                        }
                    }
                    CsvName = Path.GetFileNameWithoutExtension(filePath);
                }
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
                throw new Exception($"Error loading CSV: {ex.Message}", ex);
            }
        }

        public void SaveToCsv(string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    if (Headers.Any())
                    {
                        writer.WriteLine(string.Join(Separator, Headers.Select(HeaderEscape)));
                    }

                    foreach (var row in Rows)
                    {
                        writer.WriteLine(string.Join(Separator, row.Cells.Select(CellEscape)));
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception($"Access denied to file path: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new Exception($"IO error while writing file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving CSV: {ex.Message}", ex);
            }
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

        private List<string> ParseCsvLine(string line)
        {
            var cells = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == Separator)
                {
                    if (inQuotes)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        cells.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (c == '"')
                {
                    if (inQuotes && i < line.Length - 1 && line[i + 1] == '"')
                    {
                        sb.Append(c);
                        i++; // skip the next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            cells.Add(sb.ToString());

            return cells;
        }

        private string CellEscape(string cell)
        {
            if (cell.Contains(Separator) || cell.Contains("\""))
            {
                return $"\"{cell.Replace("\"", "\"\"")}\"";
            }
            return cell;
        }

        private string HeaderEscape(string header)
        {
            return CellEscape(header);
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
