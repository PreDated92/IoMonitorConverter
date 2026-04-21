# Performance Diagnostics - How to Enable

## Quick Enable/Disable

All performance diagnostics are hidden behind the `PERF_DIAGNOSTICS` conditional compilation symbol. By default, diagnostics are **OFF** for zero performance overhead.

## Enable Diagnostics

### Option 1: Via Project Properties (Recommended)
1. Right-click on the project (ConverterEngine, IoMonitorConverterWpf, or IoMonitorConverterForm)
2. Select **Properties**
3. Go to **Build** → **General**
4. Find **Conditional compilation symbols**
5. Add `PERF_DIAGNOSTICS` (separate with semicolon if there are existing symbols)
6. Rebuild the project

### Option 2: Via .csproj File
Add to the `<PropertyGroup>` section:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);PERF_DIAGNOSTICS</DefineConstants>
</PropertyGroup>
```

### Option 3: For Specific Configuration (Debug only)
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DefineConstants>$(DefineConstants);PERF_DIAGNOSTICS</DefineConstants>
</PropertyGroup>
```

## View Diagnostics Output

1. Press `Ctrl+Alt+O` to open the **Output** window
2. Select **"Debug"** from the dropdown
3. Run the application (F5)
4. Open an XML file
5. See detailed timing in the Output window

## What You'll See (When Enabled)

```
[ENGINE-PERF] File: example.xml, Size: 12.94 MB
[ENGINE-PERF] XML Load: 575ms
[ENGINE-PERF] Parse Limited (30 nodes): 1ms, Found 30 records
[ENGINE-PERF] Merge: 0ms, Result: 14 merged pairs
[ENGINE-PERF] Transform: 1ms
[ENGINE-PERF] CSV Generation: 0ms
[ENGINE-PERF] TOTAL Preview Time: 587ms
[WinForms-PERF] UI Setup: 9ms
[WinForms-PERF] Engine Call: 605ms
[WinForms-PERF] UI Update: 5ms
[WinForms-PERF] TOTAL UI Time: 622ms
```

## Disable Diagnostics (Default)

Remove `PERF_DIAGNOSTICS` from the compilation symbols, or simply rebuild without it.

## Performance Impact

- **Disabled (default)**: Zero overhead - all diagnostic code is completely removed at compile time
- **Enabled**: ~2-5ms overhead from Stopwatch and Debug.WriteLine calls

## Files with Diagnostics Code

- `ConverterEngine/MainEngine.cs` - Engine-level phase timing
- `IoMonitorConverterWpf/MainWindowViewModel.cs` - WPF UI timing
- `IoMonitorConverterForm/Form1.cs` - WinForms UI timing

## Key Finding from Last Run

**Bottleneck Identified:**
- File: 12.94 MB XML
- XML Load: **575ms** (98% of total time)
- All other phases: <10ms

**Next Optimization:**
Implement XML streaming to avoid loading entire file into memory.
