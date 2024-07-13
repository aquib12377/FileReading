# File Reading Library

This library provides robust functionality for reading and manipulating different types of files, including text files, CSV files, and Excel files. It also includes an improved file reading mechanism using `BufferedStream` to enhance performance.

## Features

- **TextDocument**: Handles basic text file operations.
- **CSVDocument**: Manages CSV files, including parsing and data manipulation.
- **ExcelDocument**: Provides comprehensive support for Excel files (both `.xls` and `.xlsx`), utilizing the EPPlus library for reading and writing.
- **Improved File Reading**: Uses `BufferedStream` for efficient file reading.

## Installation

To use this library in your project, you need to add the required NuGet packages:

```bash
dotnet add package CsvHelper
dotnet add package ExcelDataReader
dotnet add package EPPlus
```

## Usage

### TextDocument

The `TextDocument` class provides methods to read and write text files.

#### Example

```csharp
var textDocument = new TextDocument("example.txt");
var lines = textDocument.ReadAllLines();

foreach (var line in lines)
{
    Console.WriteLine(line);
}

textDocument.WriteAllLines(new List<string> { "Line 1", "Line 2" });
```

### CSVDocument

The `CSVDocument` class handles CSV files, allowing you to read and write CSV data easily.

#### Example

```csharp
var csvDocument = new CSVDocument("example.csv");
var rows = csvDocument.ReadAllRows();

foreach (var row in rows)
{
    Console.WriteLine(string.Join(", ", row.Cells));
}

csvDocument.WriteAllRows(new List<CSVRow>
{
    new CSVRow(new List<string> { "Column1", "Column2" }),
    new CSVRow(new List<string> { "Value1", "Value2" })
});
```

### ExcelDocument

The `ExcelDocument` class provides comprehensive support for Excel files, including reading and writing data across multiple sheets.

#### Example

```csharp
var excelDocument = new ExcelDocument("example.xlsx");
excelDocument.LoadFromFile("example.xlsx");

var sheet = excelDocument.GetSheet("Sheet1");

foreach (var row in sheet.Rows)
{
    Console.WriteLine(string.Join(", ", row.Cells));
}

excelDocument.SaveToFile("example_modified.xlsx");
```

### Improved File Reading with BufferedStream

The library includes enhanced file reading using `BufferedStream` for efficient reading operations, especially for large files.

#### Example

```csharp
using (var stream = new BufferedStream(File.OpenRead("example.txt")))
{
    using (var reader = new StreamReader(stream))
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            Console.WriteLine(line);
        }
    }
}
```

## Classes

### TextDocument

- **Methods**:
  - `ReadAllLines()`: Reads all lines from the text file.
  - `WriteAllLines(IEnumerable<string> lines)`: Writes all lines to the text file.

### CSVDocument

- **Methods**:
  - `ReadAllRows()`: Reads all rows from the CSV file.
  - `WriteAllRows(IEnumerable<CSVRow> rows)`: Writes all rows to the CSV file.

### ExcelDocument

- **Methods**:
  - `LoadFromFile(string filePath)`: Loads an Excel file.
  - `SaveToFile(string filePath)`: Saves the Excel file.
  - `GetSheet(string sheetName)`: Retrieves a specific sheet from the Excel file.

### CSVRow

- Represents a row in a CSV file.
- **Properties**:
  - `List<string> Cells`: The cells of the row.

### ExcelRow

- Represents a row in an Excel sheet.
- **Properties**:
  - `int RowNumber`: The row number.
  - `List<string> Cells`: The cells of the row.

### ExcelSheet

- Represents a sheet in an Excel file.
- **Properties**:
  - `string SheetName`: The name of the sheet.
  - `List<ExcelRow> Rows`: The rows of the sheet.
  - `List<string> Headers`: The headers of the sheet.

## Contributing

Contributions are welcome! Please submit a pull request or open an issue to discuss your ideas.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
