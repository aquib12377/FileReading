using Files;
using System;
using System.Collections.Generic;
using System.Data;

namespace FileReading.Test
{
    public class ExcelReaderTest : FileReadingTest
    {
        public List<List<List<string>>> List { get; set; }
        public DataSet DataSet { get; set; }
        public DataTableCollection DataTable { get; set; }
        public void Test()
        {
            var file = new ExcelReader()
            {
                FilePath = @"D:\Visual_Studio_Projects\FileReading\FileReading.Test\Test_Files\SampleExcel.xlsx",
                FileType = FileType.EXCEL
            };
            List = file.ConvertExcelToList();
            DataSet = file.ConvertExcelToDataSet();
            DataTable = file.ConvertExcelToDataTable();
        }
    }
}
