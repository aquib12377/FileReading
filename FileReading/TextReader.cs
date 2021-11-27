using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Files
{
    public class TextReader : FileReading
    {
        public TextReader()
        {

        }

        public TextReader(string filePath, FileType fileType, string seperator = "") : base(filePath, fileType, seperator)
        {
        }
        public List<string> ReadText (bool removeWhiteSpace = true)
        {
            var file = this;

            if (!ValidateFileData(file)) 
                throw new Exception("Invalid File Data");

            return 
                File.ReadAllLines(file.FilePath).ToList()
                .Where(a => removeWhiteSpace == false ? !string.IsNullOrEmpty(a.Trim())
                : !(removeWhiteSpace && string.IsNullOrEmpty(a.Trim()))).ToList();
        }
        public List<List<string>> ReadTextWithSeperator(bool removeWhiteSpace = true)
        {
            var file = this;
            List<string> lines = new List<string>();
            List<List<string>> textFile = new List<List<string>>();

            List<string> data = File.ReadAllLines(file.FilePath).ToList();
            
            return 
                data.Select(x => x.Trim().Split(Convert.ToChar(file.Seperator))
                .Where(a => removeWhiteSpace == false ? !string.IsNullOrEmpty(a.Trim())
                : !(removeWhiteSpace && string.IsNullOrEmpty(a.Trim()))).AsParallel()
                .Select(a => a).ToList()).ToList(); ;
        }
    }
}
