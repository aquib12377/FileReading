using Files.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Files
{
    public class TextReader : FileReading
    {
        public TextReader() { }

        public TextReader(string filePath, FileType fileType, string separator = "")
            : base(filePath, fileType, separator) { }

        /// <summary>
        /// Reads text file and returns a formatted TextDocument with pagination
        /// </summary>
        public TextDocument ReadTextFile(bool removeWhiteSpace = true, int linesPerPage = 50)
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var lines = ReadFileUsingBufferedStream(FilePath)
                .Where(line => !removeWhiteSpace || !string.IsNullOrWhiteSpace(line))
                .ToList();

            TextDocument document = new TextDocument();
            TextPage currentPage = new TextPage(1);
            int lineNumber = 1;

            foreach (var line in lines)
            {
                if (currentPage.Lines.Count == linesPerPage)
                {
                    document.AddPage(currentPage);
                    currentPage = new TextPage(currentPage.PageNumber + 1);
                }

                currentPage.AddLine(new TextLine(lineNumber, currentPage.PageNumber, line));
                lineNumber++;
            }

            if (currentPage.Lines.Count > 0)
            {
                document.AddPage(currentPage);
            }

            return document;
        }

        /// <summary>
        /// Reads text file and returns formatted output as DataTable
        /// </summary>
        public DataTable ReadTextAsDataTable(int fixedWidth = 0, char delimiter = '\t')
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var dataTable = new DataTable(Path.GetFileNameWithoutExtension(FilePath));
            var lines = ReadFileUsingBufferedStream(FilePath);
            
            // Determine columns based on first line
            var firstLine = lines.FirstOrDefault();
            if (firstLine == null)
                return dataTable;

            if (fixedWidth > 0)
            {
                // Fixed width columns
                int columnCount = (int)Math.Ceiling((double)firstLine.Length / fixedWidth);
                for (int i = 0; i < columnCount; i++)
                {
                    dataTable.Columns.Add($"Column{i + 1}", typeof(string));
                }
            }
            else
            {
                // Delimiter-based columns
                var columns = firstLine.Split(delimiter);
                for (int i = 0; i < columns.Length; i++)
                {
                    dataTable.Columns.Add($"Column{i + 1}", typeof(string));
                }
            }

            // Add rows
            foreach (var line in lines)
            {
                var dataRow = dataTable.NewRow();
                string[] values;

                if (fixedWidth > 0)
                {
                    values = new string[(int)Math.Ceiling((double)line.Length / fixedWidth)];
                    for (int i = 0; i < values.Length; i++)
                    {
                        int start = i * fixedWidth;
                        int length = Math.Min(fixedWidth, line.Length - start);
                        values[i] = start < line.Length ? line.Substring(start, length).Trim() : "";
                    }
                }
                else
                {
                    values = line.Split(delimiter);
                }

                for (int i = 0; i < Math.Min(values.Length, dataTable.Columns.Count); i++)
                {
                    dataRow[i] = values[i];
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <summary>
        /// Reads text file and returns content as a single formatted string
        /// </summary>
        public string ReadTextAsString(bool trimLines = false, bool normalizeWhitespace = false)
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var lines = ReadFileUsingBufferedStream(FilePath);
            
            if (trimLines)
            {
                lines = lines.Select(l => l.Trim()).ToList();
            }

            if (normalizeWhitespace)
            {
                lines = lines.Select(l => Regex.Replace(l, @"\s+", " ")).ToList();
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Reads text file and returns list of paragraphs (blocks separated by empty lines)
        /// </summary>
        public List<string> ReadTextAsParagraphs()
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var paragraphs = new List<string>();
            var currentParagraph = new StringBuilder();
            var lines = ReadFileUsingBufferedStream(FilePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (currentParagraph.Length > 0)
                    {
                        paragraphs.Add(currentParagraph.ToString().Trim());
                        currentParagraph.Clear();
                    }
                }
                else
                {
                    if (currentParagraph.Length > 0)
                        currentParagraph.Append(" ");
                    currentParagraph.Append(line.Trim());
                }
            }

            if (currentParagraph.Length > 0)
            {
                paragraphs.Add(currentParagraph.ToString().Trim());
            }

            return paragraphs;
        }

        /// <summary>
        /// Reads text file and extracts key-value pairs based on pattern
        /// </summary>
        public Dictionary<string, string> ExtractKeyValuePairs(string pattern = @"^(.+?):\s*(.+)$")
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var result = new Dictionary<string, string>();
            var lines = ReadFileUsingBufferedStream(FilePath);
            var regex = new Regex(pattern, RegexOptions.Multiline);

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();
                    if (!result.ContainsKey(key))
                    {
                        result[key] = value;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Reads text file and searches for lines matching a pattern
        /// </summary>
        public List<(int LineNumber, string Content)> SearchText(string pattern, bool useRegex = false)
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var results = new List<(int, string)>();
            var lines = ReadFileUsingBufferedStream(FilePath);
            
            Regex regex = null;
            if (useRegex)
            {
                try
                {
                    regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
                catch (ArgumentException ex)
                {
                    throw new Exception($"Invalid regex pattern: {ex.Message}", ex);
                }
            }

            int lineNumber = 1;
            foreach (var line in lines)
            {
                bool isMatch = useRegex 
                    ? regex.IsMatch(line) 
                    : line.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;

                if (isMatch)
                {
                    results.Add((lineNumber, line));
                }
                lineNumber++;
            }

            return results;
        }

        /// <summary>
        /// Reads large text files using streaming for better memory efficiency
        /// </summary>
        public IEnumerable<string> ReadTextLargeFile(int chunkSize = 1000)
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var chunk = new List<string>();
            
            using var reader = new StreamReader(FilePath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                chunk.Add(line);
                if (chunk.Count >= chunkSize)
                {
                    yield return chunk;
                    chunk = new List<string>();
                }
            }

            if (chunk.Any())
            {
                yield return chunk;
            }
        }

        /// <summary>
        /// Gets statistics about the text file
        /// </summary>
        public TextStatistics GetTextStatistics()
        {
            if (!ValidateFileData(this))
                throw new Exception("Invalid File Data");

            var lines = ReadFileUsingBufferedStream(FilePath);
            var stats = new TextStatistics
            {
                LineCount = lines.Count,
                CharacterCount = lines.Sum(l => l.Length),
                WordCount = lines.Sum(l => l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length),
                NonEmptyLineCount = lines.Count(l => !string.IsNullOrWhiteSpace(l))
            };

            if (lines.Any())
            {
                stats.MaxLineLength = lines.Max(l => l.Length);
                stats.AverageLineLength = (int)lines.Average(l => l.Length);
            }

            return stats;
        }
    }

    /// <summary>
    /// Statistics about a text file
    /// </summary>
    public class TextStatistics
    {
        public int LineCount { get; set; }
        public int NonEmptyLineCount { get; set; }
        public int CharacterCount { get; set; }
        public int WordCount { get; set; }
        public int MaxLineLength { get; set; }
        public int AverageLineLength { get; set; }

        public override string ToString()
        {
            return $"Lines: {LineCount}, Words: {WordCount}, Characters: {CharacterCount}, Avg Line Length: {AverageLineLength}";
        }
    }
}
