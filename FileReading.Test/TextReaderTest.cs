using Files;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileReading.Test
{
    public class TextReaderTest : FileReadingTest
    {
        public List<string> List { get; set; }
        public List<List<string>> ListWithSeperator { get; set; }
        public void Test()
        {
            var file = new TextReader()
            {
                FilePath = @"C:\Users\ITAdmin\Source\Repos\FileReading\FileReading.Test\Test_Files\SampleText.txt",
                FileType = FileType.TEXT,
                Seperator = "\t"
            };
            var textDocument = file.ReadTextFile();
        }
    }
}
