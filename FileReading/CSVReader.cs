using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Files
{
    public class CSVReader : FileReading
    {
        public CSVReader()
        {
        }

        public CSVReader(string filePath, FileType fileType, string seperator = "") : base(filePath, fileType, seperator)
        {
        }
        /// <summary>
        /// Get data from CSV with each row as string.
        /// </summary>
        /// <returns></returns>
        public List<string> ReadCSV()
        {
            if(!ValidateFileData(this)) throw new Exception("Invalid File Data");
            switch(this.FileType)
            {
                case FileType.TEXT:
                    throw new Exception("This method cannot get data from Text File");
                case FileType.EXCEL:
                    throw new Exception("This method cannot get data from Excel File");
            }
            try
            {
                var data = File.ReadAllLines(this.FilePath).ToList();
                return data;
            }
            catch (Exception _)
            {

                throw new Exception(_.Message??_.InnerException.ToString());
            }
        }
        /// <summary>
        /// Get data from CSV with each row seperated by the given seperator.
        /// </summary>
        /// <returns></returns>
        public List<string[]> ReadCSVWithSeperator()
        {
            if (!ValidateFileData(this)) throw new Exception("Invalid File Data");
            switch (this.FileType)
            {
                case FileType.TEXT:
                    throw new Exception("This method cannot get data from Text File");
                case FileType.EXCEL:
                    throw new Exception("This method cannot get data from Excel File");
            }
            try
            {
                var data = File.ReadAllLines(this.FilePath).Select(x => 
                x.Split(string.IsNullOrEmpty(this.Seperator) ? "," : this.Seperator)).ToList();
                return data;
            }
            catch (Exception _)
            {
                throw new Exception(_.Message ?? _.InnerException.ToString());
            }
        }

    }
}
