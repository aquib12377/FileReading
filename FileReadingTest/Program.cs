
using Files;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace FileReadingTest
{
    class Program
    {
        static void Main(string[] args)
        { 
            ExcelReader excelfile = new(@"C:\Users\p.patil\Downloads\TEJO-MIG-FlatFile.xlsx", FileType.EXCEL);
            TextReader textFile = new(@"C:\Users\p.patil\Downloads\2006.txt", FileType.TEXT,"|");
            Stopwatch sw = new();
            sw.Start();
            var textData = textFile.GetDataFromText(textFile);
            DataTableCollection excel = excelfile.ConvertExcelToDataTable(excelfile);
            var timeTakenDT = "Total time for Datatable Milliseconds : " + sw.ElapsedMilliseconds;
            List<List<List<string>>> excel1 = excelfile.ConvertExcelToList(excelfile);
            DataSet excel2 = excelfile.ConvertExcelToDataSet(excelfile);
            sw.Stop();
            var totalTimeTaken = "Total time in Milliseconds : " + sw.ElapsedMilliseconds;
            }
    }
}
