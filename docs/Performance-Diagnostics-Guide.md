# Performance Diagnostics Guide

## ⚠️ Diagnostics Are Hidden By Default

Performance diagnostics are **OFF by default** for zero overhead. To enable them, add the `PERF_DIAGNOSTICS` compilation symbol to your project.

See **[How-To-Enable-Diagnostics.md](How-To-Enable-Diagnostics.md)** for detailed instructions.

## How to View Diagnostics

### In Visual Studio
1. Press `Ctrl+Alt+O` to open the **Output** window
2. Select **"Debug"** from the dropdown menu at the top
3. Run the WinForms or WPF application (F5 or Ctrl+F5)
4. Open an XML file to trigger the preview
5. Watch the timing data appear in real-time

## Output Format

You'll see three levels of diagnostics:

### 1. ENGINE Level (Most Detailed)
```
[ENGINE-PERF] File: example.xml, Size: 2.45 MB
[ENGINE-PERF] XML Load: 150ms
[ENGINE-PERF] Parse Limited (30 nodes): 420ms, Found 30 records
[ENGINE-PERF] Merge: 2ms, Result: 15 merged pairs
[ENGINE-PERF] Transform: 8ms
[ENGINE-PERF] CSV Generation: 0ms
[ENGINE-PERF] TOTAL Preview Time: 580ms
```

### 2. UI Level (WinForms or WPF)
```
[WinForms-PERF] UI Setup: 13ms
[WinForms-PERF] Engine Call: 580ms
[WinForms-PERF] UI Update: 4ms
[WinForms-PERF] TOTAL UI Time: 597ms
```

## What Each Phase Means

| Phase | What It Does | Expected Time | Red Flag |
|-------|--------------|---------------|----------|
| **File Info** | Gets file size | <1ms | N/A |
| **XML Load** | Reads file from disk and parses XML DOM | 50-200ms for <10MB | >500ms |
| **Parse Limited** | Extracts first 30 Method_Enter/Exit nodes | 1-50ms | >100ms |
| **Merge** | Matches Enter/Exit pairs | <5ms for 30 nodes | >20ms |
| **Transform** | Converts timestamps and hex values | 2-20ms | >50ms |
| **CSV Generation** | Builds CSV string | <5ms | >10ms |
| **UI Binding** | Updates TextBox/RichTextBox | 2-20ms | >100ms |

## Bottleneck Identification

### If XML Load is Slow (>500ms)
- **Cause**: Large file or slow disk
- **Check**: File size in the output
- **Solution**: Consider streaming parser or file size limit

### If Parse Limited is Slow (>100ms)
- **Cause**: Complex XML structure or many attributes/parameters
- **Check**: "Found X records" count - should be ~30
- **Solution**: If finding way more than 30 records, the `.Take()` isn't working

### If Merge is Slow (>20ms for 30 nodes)
- **Cause**: O(n²) algorithm with large dataset
- **Check**: "Result: X merged pairs" - should be ~15
- **Solution**: Algorithm needs optimization if processing more records

### If Transform is Slow (>50ms)
- **Cause**: Timestamp conversion or hex decoding
- **Solution**: Cache boot time calculation

### If UI Binding is Slow (>100ms)
- **Cause**: WinForms/WPF rendering large text
- **Solution**: Virtual text or shorter preview

## Example Analysis

Given this output:
```
[ENGINE-PERF] File: trace_20240315.xml, Size: 8.32 MB
[ENGINE-PERF] XML Load: 450ms         ← 🟡 High but acceptable for 8MB
[ENGINE-PERF] Parse Limited (30 nodes): 380ms  ← 🔴 TOO SLOW! Should be <50ms
[ENGINE-PERF] Merge: 2ms              ← ✅ Good
[ENGINE-PERF] Transform: 5ms          ← ✅ Good
[ENGINE-PERF] CSV Generation: 0ms     ← ✅ Good
```

**Diagnosis**: Parse is the bottleneck (380ms out of 450ms total)
**Likely Cause**: `.Take(30)` isn't limiting properly - parsing more than 30 nodes

## Next Steps

1. Run your application
2. Open a typical XML file
3. Copy the diagnostics output from the Debug window
4. Share it for analysis
5. We'll identify the exact bottleneck and optimize

## Quick Reference

**Fast Preview (Target):**
- Total time: <200ms
- XML Load: <150ms
- Parse: <50ms
- Other phases: <10ms each

**Your Current Performance:**
- Total time: ~630ms ❌
- Need to identify which phase is slow
