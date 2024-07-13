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
            var file = new CSVReader
                (
                filePath: "C:\\Users\\ITAdmin\\source\\repos\\FileReading\\FileReading.Test\\Test_Files\\normal.csv",
                 fileType: FileType.CSV
            );
            var normalCSVDocument = file.ReadCSV();
            file.FilePath = "C:\\Users\\ITAdmin\\source\\repos\\FileReading\\FileReading.Test\\Test_Files\\dataValidation.csv";
            var commaInValCSVDocument = file.ReadCSV();
        }
    }
}
