using Files;
using Files.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;

namespace FileReading.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TextReaderTest textReaderTest = new TextReaderTest();
            textReaderTest.Test();

            CSVReaderTest cSVReaderTest = new CSVReaderTest();
            cSVReaderTest.Test();

            ExcelReaderTest excelReaderTest = new ExcelReaderTest();
            excelReaderTest.Test();
        }
    }
}
