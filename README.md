
# FileReading Helper

FileReading is a Console Application to read EXCEL, CSV and TEXT Files

## How to use?


![App Screenshot](https://raw.githubusercontent.com/aquib12377/FileReading/master/image.png)

Just create an instance of CSVReader.

```csharp
CSVReader reader = new CSVReader("Path_to_File", FileType.CSV);

var csvData = reader.ReadCSV(); ///returns List<string>
var csvDataWithSeperator = reader.ReadCSVWithSeperator(); ///returns List<string[]>
```

You can also use custom seperators.

```csharp
CSVReader reader = new CSVReader("Path_to_File", FileType.CSV, ",");
```
## Features

- Read Excel files with multiple sheets.
- Read CSV and TEXT file with or without seperators.
- Default Seperator for CSV is comma (",").


## Contributing

Contributions are always welcome!



## Authors

- [Mohammed Aquib](https://github.com/aquib12377)


## License

[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/tterb/atomic-design-ui/blob/master/LICENSEs)
