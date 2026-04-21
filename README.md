# IoMonitorConverter

A .NET 10 tool for converting Keysight IO Monitor XML trace files into CSV format for easier analysis.

## Overview

IoMonitorConverter parses XML files produced by IO monitoring tools, merges paired `Method_Enter`/`Method_Exit` records, and outputs structured CSV files. It calculates elapsed time in milliseconds and handles hex-encoded buffer values automatically.

The solution is split into a reusable core library and two UI front-ends:

| Project | Type | Description |
|---|---|---|
| `ConverterEngine` | Class library | Core parsing, merging, transformation, and CSV writing logic |
| `IoMonitorConverterWpf` | WPF application | MVVM-based desktop UI |
| `IoMonitorConverterForm` | WinForms application | Alternative Windows Forms desktop UI |
| `ConverterEngine.Tests` | xUnit test project | Unit and integration tests for the core engine |

## Features

- Streaming XML parsing for large trace files
- Merges `Method_Enter` and `Method_Exit` pairs into single rows
- Computes elapsed time (nanoseconds → milliseconds)
- Converts hex-encoded buffer values (`BinHexValue`) to ASCII
- Timestamps converted to Singapore Standard Time (configurable)
- CSV preview before saving
- Remembers the last used XML folder across sessions
- Cancel support for long-running conversions

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows (WPF and WinForms UIs are Windows-only)

## Getting Started

### Build

```powershell
dotnet build
```

### Run (WPF)

```powershell
dotnet run --project IoMonitorConverterWpf
```

### Run (WinForms)

```powershell
dotnet run --project IoMonitorConverterForm
```

### Run Tests

```powershell
dotnet test
```

## Usage

1. Launch either the WPF or WinForms application.
2. Browse to a folder containing IO Monitor XML trace files.
3. Select an XML file from the list to preview the converted CSV output.
4. Choose an output CSV file path.
5. Click **Convert** to generate the CSV file.

## Project Structure

```
IoMonitorConverter/
├── ConverterEngine/              # Core library (UI-agnostic)
│   ├── MainEngine.cs             # Conversion facade
│   ├── XmlStreamingParser.cs     # Streaming XML reader
│   ├── RecordMerger.cs           # Enter/Exit record pairing
│   ├── RecordTransformer.cs      # Field transformation
│   ├── CsvWriter.cs              # CSV output
│   ├── XmlFileInfo.cs            # File metadata model
│   └── UserSettings.cs           # Persisted settings (settings.json)
├── IoMonitorConverterWpf/        # WPF front-end (MVVM)
├── IoMonitorConverterForm/       # WinForms front-end
└── ConverterEngine.Tests/        # xUnit tests
```

## Settings

User settings (such as the last opened XML folder) are saved to `settings.json` in the application's base directory and are loaded automatically on next launch.