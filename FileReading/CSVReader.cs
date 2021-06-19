using System;
using System.Collections.Generic;
using System.Text;

namespace Files
{
    public class CSVReader : FileReading
    {
        public CSVReader(string filePath, FileType fileType, string seperator = "") : base(filePath, fileType, seperator)
        {
        }


    }
}
