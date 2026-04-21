namespace ConverterEngine
{
    /// <summary>
    /// Handles merging of Method_Enter and Method_Exit records into single output records.
    /// Correlates pairs by ThreadID only, as Enter/Exit pairs are guaranteed in sequential order.
    /// </summary>
    internal sealed class RecordMerger
    {
        private const string XmlNodeMethodEnter = "Method_Enter";
        private const string XmlNodeMethodExit = "Method_Exit";
        private const double NanosecondsToMilliseconds = 1_000_000.0;
        private const string TimeMillisecondsFormat = "F3";

        /// <summary>
        /// Merges Method_Enter and Method_Exit records into single rows.
        /// Ensures each Method_Exit is only matched once to prevent duplicate records.
        /// </summary>
        /// <param name="records">The parsed records</param>
        /// <returns>List of merged records</returns>
        public List<MergedRecord> Merge(List<MethodRecord> records)
        {
            var merged = new List<MergedRecord>();
            var usedExitIndices = new HashSet<int>();

            for (int i = 0; i < records.Count; i++)
            {
                var enter = records[i];
                if (enter.NodeType != XmlNodeMethodEnter)
                    continue;

                // Find the first unused matching exit (by ThreadID only, as pairs are guaranteed in order)
                int exitIndex = -1;
                for (int j = i + 1; j < records.Count; j++)
                {
                    if (usedExitIndices.Contains(j))
                        continue; // Skip already-used exits

                    var exit = records[j];
                    if (exit.NodeType == XmlNodeMethodExit &&
                        exit.ThreadId == enter.ThreadId)
                    {
                        exitIndex = j;
                        break;
                    }
                }

                if (exitIndex == -1)
                    continue;

                usedExitIndices.Add(exitIndex); // Mark this exit as used
                var matchedExit = records[exitIndex];

                var row = new MergedRecord
                {
                    Timestamp = enter.Timestamp,
                    AppName = enter.AppName,
                    Address = enter.Address,
                    TraceSource = enter.TraceSource,
                    MethodName = enter.MethodName,
                    ReturnValue = enter.ParamVi,
                    ParamBuf = enter.ParamBuf
                };

                if (!string.IsNullOrEmpty(matchedExit.ParamBuf))
                    row.ParamBuf = matchedExit.ParamBuf;

                if (long.TryParse(matchedExit.ElapsedNs, out long ns))
                {
                    row.TimeMs = (ns / NanosecondsToMilliseconds).ToString(TimeMillisecondsFormat, System.Globalization.CultureInfo.InvariantCulture);
                }

                merged.Add(row);
            }

            return merged;
        }
    }
}
