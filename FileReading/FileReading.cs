using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
        public FileReading(string filePath, FileType fileType, string seperator)
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
        /// <summary>
        /// Reads all lines from a file using a StreamReader and returns them as a list of strings.
        /// </summary>
        /// <param name="filePath">The path to the file to be read.</param>
        /// <returns>A list of strings, each representing a line in the file.</returns>
        /// <exception cref="IOException">Thrown when there is an issue IO Operation.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file is not present in the specified path.</exception>
        public static List<string> ReadFileUsingStreamReader(string filePath)
        {
            var lines = new List<string>(); // Initialize a list to hold the lines from the file.

            try
            {
                // Open the file using a StreamReader.
                using var reader = new StreamReader(filePath);
                string line;
                // Read each line until the end of the file.
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line); // Add the line to the list.
                }
            }
            catch (FileNotFoundException ex)
            {
                // Handle the case where the file is not found.
                throw new Exception($"File not found: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                // Handle other IO exceptions that might occur.
                throw new Exception($"IO error while reading file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that might occur.
                throw new Exception($"Error reading file: {ex.Message}", ex);
            }
            ClearCache();
            return lines; // Return the list of lines read from the file.
        }


        /// <summary>
        /// Reads all lines from a file using a BufferedStream for improved performance and returns them as a list of strings.
        /// </summary>
        /// <param name="filePath">The path to the file to be read.</param>
        /// <returns>A list of strings, each representing a line in the file.</returns>
        /// <exception cref="IOException">Thrown when there is an issue IO Operation.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file is not present in the specified path.</exception>
        public static List<string> ReadFileUsingBufferedStream(string filePath)
        {
            var lines = new List<string>(); // Initialize a list to hold the lines from the file.

            try
            {
                // Open the file with a FileStream.
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                // Wrap the FileStream with a BufferedStream for improved performance.
                using var bufferedStream = new BufferedStream(fileStream);
                // Use a StreamReader to read from the BufferedStream.
                using var reader = new StreamReader(bufferedStream);

                string line;
                // Read each line until the end of the file.
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line); // Add the line to the list.
                }
            }
            catch (FileNotFoundException ex)
            {
                // Handle the case where the file is not found.
                throw new Exception($"File not found: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                // Handle other IO exceptions that might occur.
                throw new Exception($"IO error while reading file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that might occur.
                throw new Exception($"Error reading file: {ex.Message}", ex);
            }
            ClearCache();
            return lines; // Return the list of lines read from the file.
        }
        /// <summary>
        /// Reads all lines from a file using a MemoryMappedFile for improved performance and returns them as a list of strings.
        /// </summary>
        /// <param name="filePath">The path to the file to be read.</param>
        /// <returns>A list of strings, each representing a line in the file.</returns>
        /// <exception cref="Exception">Thrown when there is an issue with reading the file.</exception>
        public static List<string> ReadFileUsingMemoryMappedFile(string filePath)
        {
            var lines = new List<string>(); // Initialize a list to hold the lines from the file.

            try
            {
                // Open the memory-mapped file.
                using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
                using var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
                using var reader = new StreamReader(stream);

                string line;
                // Read each line until the end of the file.
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line); // Add the line to the list.
                }
            }
            catch (FileNotFoundException ex)
            {
                // Handle the case where the file is not found.
                throw new Exception($"File not found: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                // Handle other IO exceptions that might occur.
                throw new Exception($"IO error while reading file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that might occur.
                throw new Exception($"Error reading file: {ex.Message}", ex);
            }
            ClearCache();
            return lines; // Return the list of lines read from the file.
        }
        private static void ClearCache()
        {}
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
