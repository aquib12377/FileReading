using Files;
using System;

namespace FileReading.Test
{
    public class FileReadingTest
    {
        private string Path { get; set; }
        private FileType Type { get; set; }
        private string Seperator { get; set; }

        public ExcelReader GetExcelReader()
        {
            try
            {
                return new ExcelReader()
                {
                    FilePath = @"D:\Visual_Studio_Projects\FileReading\FileReading.Test\Test_Files\SampleExcel.xlsx",
                    FileType = FileType.EXCEL
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public CSVReader GetCSVReader()
        {
            try
            {
                return new CSVReader()
                {
                    FilePath = @"D:\Visual_Studio_Projects\FileReading\FileReading.Test\Test_Files\SampleCSV.csv",
                    FileType = FileType.CSV
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public TextReader GeTextReader()
        {
            try
            {
                return new TextReader()
                {
                    FilePath = @"D:\Visual_Studio_Projects\FileReading\FileReading.Test\Test_Files\SampleText.txt",
                    FileType = FileType.TEXT,
                    Seperator = " "
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}