using ExcelDataReader;
using Files;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Files
{
    public class ExcelReader : FileReading
    {
        public ExcelReader(string filePath, FileType fileType, string seperator = "") : base(filePath, fileType, seperator)
        {
        }

        public ExcelReader()
        {
        }

        public DataTableCollection ConvertExcelToDataTable(bool useHeaderRow = false)
        {
            var file = this;
            if (!ValidateFileData(file)) throw new Exception("Invalid File Data");
            switch (file.FileType)
            {
                case FileType.CSV:
                    throw new Exception("This method cannot get data from CSV File");
                case FileType.TEXT:
                    throw new Exception("This method cannot get data from Text File");
            }
            try
            {
                return ConvertExcelToDataSet(useHeaderRow).Tables;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message ?? e.InnerException.ToString());
            }
        }
        public DataSet ConvertExcelToDataSet( bool useHeaderRow = false)
        {
            try
            {
                var file = this;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using var stream = File.Open(file.FilePath, FileMode.Open, FileAccess.Read);
                IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = useHeaderRow
                    }
                };
                return reader.AsDataSet(conf);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message ?? e.InnerException.ToString());
            }
        }
        public List<List<List<string>>> ConvertExcelToList()
        {
            var file = this;
            List<List<List<string>>> Tables = new List<List<List<string>>>();
            List<List<string>> Columns;
            DataTableCollection dataTables = ConvertExcelToDataTable();

            foreach (DataTable table in dataTables)
            {
                Columns = new List<List<string>>();
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Columns.Add(table.Rows.OfType<DataRow>().Select(a => a.ItemArray[i].ToString()).ToList());
                }

                Tables.Add(Columns);
            }

            return Tables;
        }
    }
}
