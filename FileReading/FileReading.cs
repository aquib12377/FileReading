using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Text;

namespace Files
{
    public class FileReading
    {
        public string Seperator { get; set; }
        public string FilePath { get; set; }
        public FileType FileType { get; set; }
        public FileReading()
        {

        }
        public FileReading(string filePath, FileType fileType, string seperator = "")
        {
            FilePath = filePath;
            FileType = fileType;
            Seperator = seperator;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileReading object"></param>
        /// <returns>True if file data is correct else False</returns>
        public bool ValidateFileData(FileReading file)
        {
            try
            {
                if (string.IsNullOrEmpty(file.FilePath)) return false;
                if (!File.Exists(file.FilePath)) return false;
                return file.FileType switch
                {
                    FileType.CSV => true,
                    FileType.EXCEL => true,
                    FileType.TEXT => true,
                    _ => false,
                };
            }
            catch
            {
                return false;
            }
        }
    }
    public enum FileType
    {
        TEXT,
        CSV,
        EXCEL
    }
    public enum GETDATAIN
    {
        BYTEARRAY,
        STRING,
        LISTSTRING
    }
}
