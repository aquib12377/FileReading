using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Files.Models
{
    public class TextLine
    {
        public int LineNumber { get; set; }
        public int PageNumber { get; set; }
        public string Content { get; set; }

        public TextLine(int lineNumber, int pageNumber, string content)
        {
            LineNumber = lineNumber;
            PageNumber = pageNumber;
            Content = content;
        }

        public override string ToString()
        {
            return $"Page {PageNumber}, Line {LineNumber}: {Content}";
        }
    }


    public class TextPage
    {
        public int PageNumber { get; set; }
        public List<TextLine> Lines { get; set; }

        public TextPage(int pageNumber)
        {
            PageNumber = pageNumber;
            Lines = new List<TextLine>();
        }

        public void AddLine(TextLine line)
        {
            Lines.Add(line);
        }

        public void RemoveLine(int lineNumber)
        {
            var line = Lines.FirstOrDefault(l => l.LineNumber == lineNumber);
            if (line != null)
            {
                Lines.Remove(line);
                ReIndexLines();
            }
        }

        public TextLine GetLine(int lineNumber)
        {
            return Lines.FirstOrDefault(l => l.LineNumber == lineNumber);
        }

        private void ReIndexLines()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].LineNumber = i + 1;
            }
        }

        public override string ToString()
        {
            return $"Page {PageNumber} ({Lines.Count} lines)";
        }
    }


    public class TextDocument
    {
        public List<TextPage> Pages { get; private set; }
        public string DocumentName { get; private set; }

        public TextDocument()
        {
            Pages = new List<TextPage>();
        }

        public void AddPage(TextPage page)
        {
            Pages.Add(page);
        }

        public void AddPages(IEnumerable<TextPage> pages)
        {
            foreach (var page in pages)
            {
                AddPage(page);
            }
        }

        public void RemovePage(int pageNumber)
        {
            var page = GetPage(pageNumber);
            if (page != null)
            {
                Pages.Remove(page);
            }
        }

        public TextLine GetLine(int lineNumber)
        {
            return Pages.SelectMany(p => p.Lines).FirstOrDefault(l => l.LineNumber == lineNumber);
        }

        public TextPage GetPage(int pageNumber)
        {
            return Pages.FirstOrDefault(p => p.PageNumber == pageNumber);
        }

        public void LoadFromTextFile(string filePath, int linesPerPage = 40)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    int lineNumber = 1;
                    int pageNumber = 1;
                    string line;
                    TextPage page = new TextPage(pageNumber);

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (page.Lines.Count >= linesPerPage)
                        {
                            AddPage(page);
                            pageNumber++;
                            page = new TextPage(pageNumber);
                        }

                        page.AddLine(new TextLine(lineNumber, pageNumber, line));
                        lineNumber++;
                    }

                    // Add the last page if it has any lines
                    if (page.Lines.Count > 0)
                    {
                        AddPage(page);
                    }
                }

                DocumentName = Path.GetFileNameWithoutExtension(filePath);
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
                throw new Exception($"Error loading text file: {ex.Message}", ex);
            }
        }

        public void SaveToTextFile(string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                foreach (var page in Pages)
                {
                    foreach (var line in page.Lines)
                    {
                        writer.WriteLine(line.Content);
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
                throw new Exception($"Error saving text file: {ex.Message}", ex);
            }
        }

        public List<TextLine> FindLinesByContent(string content)
        {
            try
            {
                var matchingLines = Pages.SelectMany(p => p.Lines).Where(l => l.Content.Contains(content)).ToList();
                if (!matchingLines.Any())
                    throw new Exception($"No lines found containing '{content}'.");

                return matchingLines;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding lines: {ex.Message}", ex);
            }
        }

        public override string ToString()
        {
            try
            {
                return $"Document with {Pages.Count} pages";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting document to string: {ex.Message}", ex);
            }
        }
    }
}
