using System.Globalization;
using System.Text;

namespace ConverterEngine
{
    /// <summary>
    /// Handles transformation of merged records: timestamp conversion and hex decoding.
    /// </summary>
    internal sealed class RecordTransformer
    {
        private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const int TicksPerHundredNanoseconds = 100;
        private const char CsvQuote = '"';
        private const char CsvComma = ',';

        /// <summary>
        /// Transforms merged records by converting timestamps and hex values.
        /// </summary>
        /// <param name="records">The merged records to transform</param>
        /// <param name="timeZoneId">Timezone ID for timestamp conversion</param>
        public void Transform(List<MergedRecord> records, string timeZoneId)
        {
            foreach (var rec in records)
            {
                rec.Timestamp = ConvertNanosecondsToTimestamp(rec.Timestamp, timeZoneId);

                if (!string.IsNullOrEmpty(rec.ParamBuf))
                {
                    try
                    {
                        var asciiValue = ConvertHex(rec.ParamBuf).TrimEnd();
                        if (asciiValue.Contains(CsvComma) && !asciiValue.StartsWith(CsvQuote))
                        {
                            asciiValue = $"{CsvQuote}{asciiValue}{CsvQuote}";
                        }
                        rec.ParamBuf = asciiValue;
                    }
                    catch (ArgumentException)
                    {
                        rec.ParamBuf = "";
                    }
                }
            }
        }

        /// <summary>
        /// Converts nanosecond offset to formatted timestamp in specified timezone.
        /// </summary>
        private static string ConvertNanosecondsToTimestamp(string nanosecondOffset, string timeZoneId)
        {
            if (!long.TryParse(nanosecondOffset, out long nanoOffset))
                return "";

            DateTime bootTimeUtc = DateTime.UtcNow.AddMilliseconds(-Environment.TickCount64);
            DateTime utcDate = bootTimeUtc.AddTicks(nanoOffset / TicksPerHundredNanoseconds);

            TimeZoneInfo targetZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, targetZone);

            return localTime.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts hex string to ASCII string.
        /// </summary>
        private static string ConvertHex(string hexString)
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
    }
}
