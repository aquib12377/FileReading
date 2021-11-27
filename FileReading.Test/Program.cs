using System;

namespace FileReading.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ExcelReaderTest excelReaderTest = new ExcelReaderTest();
            excelReaderTest.Test();

            CSVReaderTest cSVReaderTest = new CSVReaderTest();
            cSVReaderTest.Test();

            TextReaderTest textReaderTest = new TextReaderTest();
            textReaderTest.Test();
        }
    }
}
