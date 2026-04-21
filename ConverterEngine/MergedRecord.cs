namespace ConverterEngine
{
    /// <summary>
    /// Represents a merged Method_Enter/Method_Exit pair ready for CSV output.
    /// </summary>
    internal sealed class MergedRecord
    {
        public string Timestamp { get; set; } = "";
        public string AppName { get; set; } = "";
        public string Address { get; set; } = "";
        public string TraceSource { get; set; } = "";
        public string MethodName { get; set; } = "";
        public string ParamBuf { get; set; } = "";
        public string ReturnValue { get; set; } = "";
        public string TimeMs { get; set; } = "";
    }
}
