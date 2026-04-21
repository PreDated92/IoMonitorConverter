using System.Text;

namespace ConverterEngine
{
    /// <summary>
    /// Handles CSV generation from merged records.
    /// </summary>
    internal sealed class CsvWriter
    {
        private const string CsvHeader = "Timestamp,AppName,Address,TraceSource,MethodName,Param_buf,ReturnValue,Time(ms)";

        /// <summary>
        /// Generates CSV content from merged records.
        /// </summary>
        /// <param name="records">The records to write</param>
        /// <returns>CSV content as string</returns>
        public string Write(List<MergedRecord> records)
        {
            var sb = new StringBuilder();

            sb.AppendLine(CsvHeader);

            foreach (var rec in records)
            {
                sb.AppendLine(string.Join(",",
                    rec.Timestamp,
                    rec.AppName,
                    rec.Address,
                    rec.TraceSource,
                    rec.MethodName,
                    rec.ParamBuf,
                    rec.ReturnValue,
                    rec.TimeMs
                ));
            }

            return sb.ToString();
        }
    }
}
