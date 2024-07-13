using Files.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Files
{
    public class CSVReader : FileReading
    {
        public CSVReader() { }

        public CSVReader(string filePath, FileType fileType, string separator = ",") : base(filePath, fileType, separator) 
        { }

        public CsvDocument ReadCSV(bool removeWhiteSpace = true)
        {
            if (!ValidateFileData(this)) 
                throw new Exception("Invalid File Data");

            var document = new CsvDocument(FilePath);

            int lineNumber = 1;
            foreach (var line in ReadCsvLines(FilePath))
            {
                var cells = ParseCsvLine(line, Seperator);
                if (removeWhiteSpace)
                {
                    cells = cells.Where(cell => !string.IsNullOrWhiteSpace(cell)).ToList();
                }

                var row = new CSVRow(lineNumber, cells);
                document.AddRow(row);
                lineNumber++;
            }
            return document;
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

        public CsvDocument ReadCSVWithSeparator(string pageBoundaryContent = null, bool removeWhiteSpace = true)
        {
            if (!ValidateFileData(this)) throw new Exception("Invalid File Data");

            var data = ReadFileUsingBufferedStream(FilePath);
            var document = new CsvDocument(FilePath);

            int lineNumber = 1;
            foreach (var line in data)
            {
                var cells = line.Split(string.IsNullOrEmpty(this.Seperator) ? ',' : Convert.ToChar(this.Seperator)).ToList();
                if (removeWhiteSpace)
                {
                    cells = cells.Where(cell => !string.IsNullOrWhiteSpace(cell)).ToList();
                }

                var row = new CSVRow(lineNumber, cells);
                document.AddRow(row);
                lineNumber++;

                if (!string.IsNullOrEmpty(pageBoundaryContent) && cells.Contains(pageBoundaryContent))
                {
                    // Pending
                }
            }

            return document;
        }
    }

}
