using Files.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Files
{
    public class TextReader : FileReading
    {
        public TextReader() { }

        public TextReader(string filePath, FileType fileType, string separator = "")
            : base(filePath, fileType, separator) { }

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
    }

}
