using Files.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Files
{
    public class CSVReader : FileReading
    {
        public CSVReader() { }

        public CSVReader(string filePath, FileType fileType, string separator = ",") : base(filePath, fileType, separator) 
        { }

        /// <summary>
        /// Reads CSV file and returns a formatted CsvDocument
        /// </summary>
        public CsvDocument ReadCSV(bool removeWhiteSpace = true, bool hasHeaders = false, bool autoDetectDelimiter = false)
        {
            if (!ValidateFileData(this)) 
                throw new Exception("Invalid File Data");

            var document = new CsvDocument(FilePath, string.IsNullOrEmpty(Seperator) ? ',' : Seperator[0]);

            // Auto-detect delimiter if requested
            if (autoDetectDelimiter)
            {
                Seperator = DetectDelimiter(FilePath);
                document.Separator = Seperator[0];
            }

            int lineNumber = 1;
            foreach (var line in ReadCsvLines(FilePath))
            {
                var cells = ParseCsvLine(line, Seperator);
                
                if (hasHeaders && lineNumber == 1)
                {
                    document.SetHeaders(cells);
                    lineNumber++;
                    continue;
                }

                if (removeWhiteSpace)
                {
                    cells = cells.Select(c => c?.Trim()).ToList();
                }

                var row = new CSVRow(lineNumber, cells);
                document.AddRow(row);
                lineNumber++;
            }
            return document;
        }

        /// <summary>
        /// Reads CSV and returns formatted output as DataTable
        /// </summary>
        public DataTable ReadCSVAsDataTable(bool hasHeaders = true, Dictionary<string, Type> columnTypes = null)
        {
            var document = ReadCSV(hasHeaders: hasHeaders);
            var dataTable = new DataTable(Path.GetFileNameWithoutExtension(FilePath));

            // Add columns
            if (hasHeaders && document.Headers.Any())
            {
                for (int i = 0; i < document.Headers.Count; i++)
                {
                    var columnName = document.Headers[i];
                    var columnType = columnTypes?.ContainsKey(columnName) == true ? columnTypes[columnName] : typeof(string);
                    dataTable.Columns.Add(columnName, columnType);
                }
            }
            else
            {
                // Use default column names
                int columnCount = document.Rows.FirstOrDefault()?.Cells.Count ?? 0;
                for (int i = 0; i < columnCount; i++)
                {
                    dataTable.Columns.Add($"Column{i + 1}", typeof(string));
                }
            }

            // Add rows
            foreach (var row in document.Rows)
            {
                if (dataTable.Columns.Count == 0)
                {
                    // Initialize columns based on first row
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        dataTable.Columns.Add($"Column{i + 1}", typeof(string));
                    }
                }

                var dataRow = dataTable.NewRow();
                for (int i = 0; i < Math.Min(row.Cells.Count, dataTable.Columns.Count); i++)
                {
                    dataRow[i] = row.Cells[i] ?? DBNull.Value;
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <summary>
        /// Reads CSV and returns formatted output as list of dictionaries
        /// </summary>
        public List<Dictionary<string, string>> ReadCSVAsDictionary(bool hasHeaders = true)
        {
            var result = new List<Dictionary<string, string>>();
            var document = ReadCSV(hasHeaders: hasHeaders);

            if (!hasHeaders || !document.Headers.Any())
            {
                // If no headers, use index-based keys
                int columnIndex = 0;
                foreach (var row in document.Rows)
                {
                    var dict = new Dictionary<string, string>();
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        dict[$"Column{i + 1}"] = row.Cells[i];
                    }
                    result.Add(dict);
                }
            }
            else
            {
                foreach (var row in document.Rows)
                {
                    var dict = new Dictionary<string, string>();
                    for (int i = 0; i < Math.Min(row.Cells.Count, document.Headers.Count); i++)
                    {
                        dict[document.Headers[i]] = row.Cells[i];
                    }
                    result.Add(dict);
                }
            }

            return result;
        }

        /// <summary>
        /// Reads CSV and returns formatted JSON string
        /// </summary>
        public string ReadCSVAsJson(bool hasHeaders = true, bool prettyPrint = false)
        {
            var data = ReadCSVAsDictionary(hasHeaders);
            var sb = new StringBuilder();
            
            sb.AppendLine("[");
            for (int i = 0; i < data.Count; i++)
            {
                sb.AppendLine("  {");
                var items = data[i].ToList();
                for (int j = 0; j < items.Count; j++)
                {
                    var key = EscapeJsonString(items[j].Key);
                    var value = EscapeJsonString(items[j].Value ?? "");
                    sb.Append($"    \"{key}\": \"{value}\"");
                    if (j < items.Count - 1)
                        sb.Append(",");
                    sb.AppendLine();
                }
                sb.Append("  }");
                if (i < data.Count - 1)
                    sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("]");

            return prettyPrint ? sb.ToString() : sb.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", "").Replace("    ", "");
        }

        /// <summary>
        /// Reads CSV with custom page boundaries
        /// </summary>
        public CsvDocument ReadCSVWithSeparator(string pageBoundaryContent = null, bool removeWhiteSpace = true)
        {
            if (!ValidateFileData(this)) throw new Exception("Invalid File Data");

            var document = new CsvDocument(FilePath);

            int lineNumber = 1;
            foreach (var line in ReadCsvLines(FilePath))
            {
                var cells = line.Split(string.IsNullOrEmpty(this.Seperator) ? ',' : Convert.ToChar(this.Seperator)).ToList();
                if (removeWhiteSpace)
                {
                    cells = cells.Select(c => c?.Trim()).ToList();
                }

                var row = new CSVRow(lineNumber, cells);
                document.AddRow(row);
                lineNumber++;

                if (!string.IsNullOrEmpty(pageBoundaryContent) && cells.Contains(pageBoundaryContent))
                {
                    // Page boundary detected - can be used for pagination logic
                }
            }

            return document;
        }

        /// <summary>
        /// Reads large CSV files using streaming for better performance
        /// </summary>
        public IEnumerable<List<string>> ReadCSVLargeFile(int batchSize = 1000, bool hasHeaders = false)
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var batch = new List<List<string>>();
            int lineNumber = 0;
            bool headersSkipped = false;

            foreach (var line in ReadCsvLines(FilePath))
            {
                if (hasHeaders && !headersSkipped)
                {
                    headersSkipped = true;
                    continue;
                }

                var cells = ParseCsvLine(line, Seperator);
                batch.Add(cells.Select(c => c?.Trim()).ToList());
                lineNumber++;

                if (batch.Count >= batchSize)
                {
                    yield return batch;
                    batch = new List<List<string>>();
                }
            }

            if (batch.Any())
            {
                yield return batch;
            }
        }

        /// <summary>
        /// Auto-detects the delimiter used in the CSV file
        /// </summary>
        private string DetectDelimiter(string filePath)
        {
            var delimiters = new[] { ',', ';', '\t', '|' };
            var delimiterCounts = new Dictionary<char, int>();

            using (var reader = new StreamReader(filePath))
            {
                // Read first few lines to detect delimiter
                for (int i = 0; i < 5; i++)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    foreach (var delim in delimiters)
                    {
                        var count = line.Count(c => c == delim);
                        if (!delimiterCounts.ContainsKey(delim))
                            delimiterCounts[delim] = 0;
                        delimiterCounts[delim] += count;
                    }
                }
            }

            return delimiterCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key.ToString() ?? ",";
        }

        private IEnumerable<string> ReadCsvLines(string filePath)
        {
            using var reader = new StreamReader(filePath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        private List<string> ParseCsvLine(string line, string separator)
        {
            var cells = new List<string>();
            var currentCell = new StringBuilder();
            bool insideQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentCell.Append(c); // handle escaped quote
                        i++; // skip next quote
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (c == Convert.ToChar(separator) && !insideQuotes)
                {
                    cells.Add(currentCell.ToString());
                    currentCell.Clear();
                }
                else
                {
                    currentCell.Append(c);
                }
            }

            cells.Add(currentCell.ToString());
            return cells;
        }

        private string EscapeJsonString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            
            return input.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }
    }

}
