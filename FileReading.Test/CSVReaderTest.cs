using Files;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileReading.Test
{
    public class CSVReaderTest : FileReadingTest
    {
        public List<string> List { get; set; }
        public List<string[]> ListWithSeperators { get; set; }
        public void Test()
        {
            var file = new CSVReader()
            {
                FilePath = @"D:\Visual_Studio_Projects\FileReading\FileReading.Test\Test_Files\SampleCSV.csv",
                FileType = FileType.CSV
            };
            List = file.ReadCSV();
            ListWithSeperators = file.ReadCSVWithSeperator();
        }
    }
}
