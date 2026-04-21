namespace ConverterEngine
{
    /// <summary>
    /// Represents a parsed Method_Enter or Method_Exit XML node with its attributes and parameters.
    /// </summary>
    internal sealed class MethodRecord
    {
        public required string NodeType { get; init; }
        public required string ThreadId { get; init; }
        public required string Address { get; init; }
        public required string Timestamp { get; init; }
        public string AppName { get; init; } = "";
        public string TraceSource { get; init; } = "";
        public string MethodName { get; init; } = "";
        public string ParamBuf { get; init; } = "";
        public string ParamVi { get; init; } = "";
        public string ElapsedNs { get; init; } = "";
        public Dictionary<string, string> AdditionalParameters { get; init; } = new();
    }
}
