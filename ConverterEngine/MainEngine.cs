using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace ConverterEngine
{
    /// <summary>
    /// Facade for XML to CSV conversion operations.
    /// Delegates to specialized classes for parsing, merging, transformation, and CSV writing.
    /// </summary>
    public class MainEngine
    {
        private const string DefaultTimeZoneId = "Singapore Standard Time";

        // XML Element and Attribute Names
        private const string XmlNodeMethodEnter = "Method_Enter";
        private const string XmlNodeMethodExit = "Method_Exit";
        private const string XmlElementParameter = "Parameter";
        private const string XmlElementElement = "Element";
        private const string XmlAttributeName = "Name";
        private const string XmlAttributeValue = "Value";
        private const string XmlAttributeBinHexValue = "BinHexValue";

        // Field Names
        private const string FieldNodeType = "NodeType";
        private const string FieldThreadId = "ThreadID";
        private const string FieldAddress = "Address";
        private const string FieldTimestamp = "Timestamp";
        private const string FieldAppName = "AppName";
        private const string FieldTraceSource = "TraceSource";
        private const string FieldMethodName = "MethodName";
        private const string FieldParamBuf = "Param_buf";
        private const string FieldReturnValue = "ReturnValue";
        private const string FieldParamVi = "Param_vi";
        private const string FieldElapsedNs = "ElapsedNS";
        private const string FieldTimeMs = "Time(ms)";
        private const string FieldParamPrefix = "Param_";

        // Format Strings
        private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const string TimeMillisecondsFormat = "F3";

        // Conversion Constants
        private const double NanosecondsToMilliseconds = 1_000_000.0;
        private const int TicksPerHundredNanoseconds = 100;

        // CSV Constants
        private const char CsvQuote = '"';
        private const char CsvComma = ',';

        // Messages
        private const string CsvGeneratedMessage = "CSV generated: ";

        /// <summary>
        /// Converts hex string to ASCII string. Public API maintained for backward compatibility.
        /// </summary>
        public static string ConvertHex(string hexString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(hexString);

            var ascii = new StringBuilder(hexString.Length / 2);

            for (int i = 0; i < hexString.Length; i += 2)
            {
                if (i + 1 >= hexString.Length)
                    throw new ArgumentException($"Invalid hex string length at position {i}", nameof(hexString));

                string hexPair = hexString.Substring(i, 2);
                if (!uint.TryParse(hexPair, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint decval))
                    throw new ArgumentException($"Invalid hex characters '{hexPair}' at position {i}", nameof(hexString));

                ascii.Append(Convert.ToChar(decval));
            }

            return ascii.ToString();
        }

        /// <summary>
        /// Parses XML document and extracts Method_Enter and Method_Exit records.
        /// Legacy method maintained for backward compatibility with ConvertFileAsync.
        /// </summary>
        /// <param name="doc">The XML document to parse</param>
        /// <returns>List of records with node attributes and parameters</returns>
        private static List<Dictionary<string, string>> ParseXmlRecords(XDocument doc)
        {
            var records = new List<Dictionary<string, string>>();

            var methodNodes = doc.Descendants()
                .Where(x => x.Name == XmlNodeMethodEnter || x.Name == XmlNodeMethodExit);

            foreach (var node in methodNodes)
            {
                var record = new Dictionary<string, string>();

                record[FieldNodeType] = node.Name.LocalName;
                foreach (var attr in node.Attributes())
                    record[attr.Name.LocalName] = attr.Value;

                int index = 1;
                foreach (var param in node.Elements(XmlElementParameter))
                {
                    string paramName = param.Attribute(XmlAttributeName)?.Value ?? (FieldParamPrefix + index);
                    var element = param.Element(XmlElementElement);

                    string value =
                        element?.Attribute(XmlAttributeValue)?.Value ??
                        element?.Attribute(XmlAttributeBinHexValue)?.Value ??
                        "";

                    record[$"{FieldParamPrefix}{paramName}"] = value;
                    index++;
                }

                records.Add(record);
            }

            return records;
        }

        /// <summary>
        /// Converts Dictionary-based records to strongly-typed MergedRecord list.
        /// Bridge method between legacy DOM approach and new type-safe approach.
        /// </summary>
        private static List<MergedRecord> ConvertDictionaryToMergedRecords(List<Dictionary<string, string>> dictRecords)
        {
            return dictRecords.Select(dict => new MergedRecord
            {
                Timestamp = dict.GetValueOrDefault(FieldTimestamp, ""),
                AppName = dict.GetValueOrDefault(FieldAppName, ""),
                Address = dict.GetValueOrDefault(FieldAddress, ""),
                TraceSource = dict.GetValueOrDefault(FieldTraceSource, ""),
                MethodName = dict.GetValueOrDefault(FieldMethodName, ""),
                ParamBuf = dict.GetValueOrDefault(FieldParamBuf, ""),
                ReturnValue = dict.GetValueOrDefault(FieldReturnValue, ""),
                TimeMs = dict.GetValueOrDefault(FieldTimeMs, "")
            }).ToList();
        }

        /// <summary>
        /// Merges Method_Enter and Method_Exit records into single rows.
        /// Legacy method maintained for backward compatibility.
        /// Ensures each Method_Exit is only matched once to prevent duplicate records.
        /// </summary>
        /// <param name="records">The parsed records</param>
        /// <returns>List of merged records</returns>
        private static List<Dictionary<string, string>> MergeEnterExitRecords(List<Dictionary<string, string>> records)
        {
            var merged = new List<Dictionary<string, string>>();
            var usedExitIndices = new HashSet<int>();

            for (int i = 0; i < records.Count; i++)
            {
                var enter = records[i];
                if (!enter.TryGetValue(FieldNodeType, out var nodeType) || nodeType != XmlNodeMethodEnter)
                    continue;

                // Find the first unused matching exit
                int exitIndex = -1;
                for (int j = i + 1; j < records.Count; j++)
                {
                    if (usedExitIndices.Contains(j))
                        continue; // Skip already-used exits

                    var exit = records[j];
                    if (exit.TryGetValue(FieldNodeType, out var nt) && nt == XmlNodeMethodExit &&
                        exit.TryGetValue(FieldThreadId, out var tid1) &&
                        enter.TryGetValue(FieldThreadId, out var tid2) &&
                        tid1 == tid2)
                    {
                        exitIndex = j;
                        break;
                    }
                }

                if (exitIndex == -1)
                    continue;

                usedExitIndices.Add(exitIndex); // Mark this exit as used
                var matchedExit = records[exitIndex];

                var row = new Dictionary<string, string>
                {
                    [FieldTimestamp] = enter.GetValueOrDefault(FieldTimestamp, ""),
                    [FieldAppName] = enter.GetValueOrDefault(FieldAppName, ""),
                    [FieldAddress] = enter.GetValueOrDefault(FieldAddress, ""),
                    [FieldTraceSource] = enter.GetValueOrDefault(FieldTraceSource, ""),
                    [FieldMethodName] = enter.GetValueOrDefault(FieldMethodName, ""),
                    [FieldReturnValue] = enter.GetValueOrDefault(FieldParamVi, "")
                };

                if (enter.TryGetValue(FieldParamBuf, out var enterBuf))
                    row[FieldParamBuf] = enterBuf;

                if (matchedExit.TryGetValue(FieldParamBuf, out var exitBuf) && !string.IsNullOrEmpty(exitBuf))
                    row[FieldParamBuf] = exitBuf;

                if (matchedExit.TryGetValue(FieldElapsedNs, out var elapsedNs) &&
                    long.TryParse(elapsedNs, out long ns))
                {
                    row[FieldTimeMs] = (ns / NanosecondsToMilliseconds).ToString(TimeMillisecondsFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    row[FieldTimeMs] = "";
                }

                merged.Add(row);
            }

            return merged;
        }

        /// <summary>
        /// Converts an XML file to CSV asynchronously.
        /// Uses DOM-based approach for full file processing.
        /// </summary>
        /// <param name="xmlPath">Input XML file path</param>
        /// <param name="csvPath">Output CSV file path</param>
        /// <param name="timeZoneId">Optional timezone ID for timestamp conversion (defaults to Singapore Standard Time)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>The CSV content as a string</returns>
        public static async Task<string> ConvertFileAsync(string xmlPath, string csvPath, string? timeZoneId = null, CancellationToken cancellationToken = default)
        {
            XDocument doc;
            await using (var stream = File.OpenRead(xmlPath))
            {
                doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var records = ParseXmlRecords(doc);

            cancellationToken.ThrowIfCancellationRequested();

            var dictMerged = MergeEnterExitRecords(records);
            var merged = ConvertDictionaryToMergedRecords(dictMerged);

            cancellationToken.ThrowIfCancellationRequested();

            var transformer = new RecordTransformer();
            transformer.Transform(merged, timeZoneId ?? DefaultTimeZoneId);

            cancellationToken.ThrowIfCancellationRequested();

            var csvWriter = new CsvWriter();
            var csvContent = csvWriter.Write(merged);

            await using (var writer = new StreamWriter(csvPath))
            {
                await writer.WriteAsync(csvContent);
            }

            Console.WriteLine(CsvGeneratedMessage + csvPath);
            return csvContent;
        }

        /// <summary>
        /// Generates a CSV preview from an XML file without writing to disk.
        /// </summary>
        /// <param name="xmlPath">Input XML file path</param>
        /// <param name="maxRecords">Maximum number of records to include in preview (default: 100, 0 = all records)</param>
        /// <param name="timeZoneId">Optional timezone ID for timestamp conversion (defaults to Singapore Standard Time)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>The CSV preview content as a string</returns>
        public static async Task<string> GeneratePreviewAsync(string xmlPath, int maxRecords = 100, string? timeZoneId = null, CancellationToken cancellationToken = default)
        {
#if PERF_DIAGNOSTICS
            var totalWatch = System.Diagnostics.Stopwatch.StartNew();
            var phaseWatch = System.Diagnostics.Stopwatch.StartNew();

            var fileInfo = new FileInfo(xmlPath);
            var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
            System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] File: {Path.GetFileName(xmlPath)}, Size: {fileSizeMB:F2} MB");
#endif

            cancellationToken.ThrowIfCancellationRequested();

            List<MergedRecord> merged;

            if (maxRecords > 0)
            {
#if PERF_DIAGNOSTICS
                phaseWatch.Restart();
#endif
                // Use streaming parser - reads only what we need
                var parser = new XmlStreamingParser();
                var methodRecords = await parser.ParseStreamingAsync(xmlPath, maxRecords * 3, cancellationToken);
#if PERF_DIAGNOSTICS
                var parseTime = phaseWatch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] XML Stream Parse ({maxRecords * 3} nodes): {parseTime}ms, Found {methodRecords.Count} records");
#endif

                cancellationToken.ThrowIfCancellationRequested();

#if PERF_DIAGNOSTICS
                phaseWatch.Restart();
#endif
                var merger = new RecordMerger();
                merged = merger.Merge(methodRecords);
#if PERF_DIAGNOSTICS
                var mergeTime = phaseWatch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] Merge: {mergeTime}ms, Result: {merged.Count} merged pairs");
#endif

                if (merged.Count > maxRecords)
                {
                    merged = merged.Take(maxRecords).ToList();
                }
            }
            else
            {
                // Full processing - use DOM approach for backward compatibility
#if PERF_DIAGNOSTICS
                phaseWatch.Restart();
#endif
                XDocument doc;
                await using (var stream = File.OpenRead(xmlPath))
                {
                    doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
                }
#if PERF_DIAGNOSTICS
                var loadTime = phaseWatch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] XML Load (Full): {loadTime}ms");

                phaseWatch.Restart();
#endif
                var records = ParseXmlRecords(doc);
#if PERF_DIAGNOSTICS
                var parseTime = phaseWatch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] Parse Full: {parseTime}ms, Found {records.Count} records");
#endif

                cancellationToken.ThrowIfCancellationRequested();

#if PERF_DIAGNOSTICS
                phaseWatch.Restart();
#endif
                merged = ConvertDictionaryToMergedRecords(MergeEnterExitRecords(records));
#if PERF_DIAGNOSTICS
                var mergeTime = phaseWatch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] Merge: {mergeTime}ms, Result: {merged.Count} merged pairs");
#endif
            }

            cancellationToken.ThrowIfCancellationRequested();

#if PERF_DIAGNOSTICS
            phaseWatch.Restart();
#endif
            var transformer = new RecordTransformer();
            transformer.Transform(merged, timeZoneId ?? DefaultTimeZoneId);
#if PERF_DIAGNOSTICS
            var transformTime = phaseWatch.ElapsedMilliseconds;
            System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] Transform: {transformTime}ms");
#endif

            cancellationToken.ThrowIfCancellationRequested();

#if PERF_DIAGNOSTICS
            phaseWatch.Restart();
#endif
            var csvWriter = new CsvWriter();
            var csvContent = csvWriter.Write(merged);
#if PERF_DIAGNOSTICS
            var csvTime = phaseWatch.ElapsedMilliseconds;
            System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] CSV Generation: {csvTime}ms");

            totalWatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ENGINE-PERF] TOTAL Preview Time: {totalWatch.ElapsedMilliseconds}ms");
#endif

            return csvContent;
        }
    }
}
