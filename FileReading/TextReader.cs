using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Files
{
    public class TextReader : FileReading
    {
        public TextReader(string filePath, FileType fileType, string seperator = "") : base(filePath, fileType, seperator)
        {
        }
        public List<List<string>> GetDataFromText(FileReading file, bool removeWhiteSpace = true)
        {

            if (!ValidateFileData(file)) throw new Exception("Invalid File Data");
            if (!string.IsNullOrEmpty(file.Seperator.Trim())) return GetTextDataWithSeperator(file, removeWhiteSpace);
            return new List<List<string>> { File.ReadAllLines(file.FilePath).ToList().Where(a => removeWhiteSpace == false ? !string.IsNullOrEmpty(a.Trim()) : !(removeWhiteSpace && string.IsNullOrEmpty(a.Trim()))).ToList() };
        }

        private List<List<string>> GetTextDataWithSeperator(FileReading file, bool removeWhiteSpace = true)
        {
            List<string> lines = new List<string>();
            List<List<string>> textFile = new List<List<string>>();
            string data = File.ReadAllText(file.FilePath);
            if (!data.Contains(file.Seperator.Trim())) throw new Exception("Given seperator is not present in the text file");
            textFile.Add(data.Split(Convert.ToChar(file.Seperator)).Where(a => removeWhiteSpace == false ? !string.IsNullOrEmpty(a.Trim()) : !(removeWhiteSpace && string.IsNullOrEmpty(a.Trim()))).AsParallel().Select(a => a).ToList());
            return textFile;
        }
    }
}
