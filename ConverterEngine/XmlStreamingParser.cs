using System.Globalization;
using System.Xml;

namespace ConverterEngine
{
    /// <summary>
    /// Handles XML parsing using both DOM (XDocument) and streaming (XmlReader) approaches.
    /// Streaming is used for preview scenarios to avoid loading entire file into memory.
    /// </summary>
    internal sealed class XmlStreamingParser
    {
        private const string XmlNodeMethodEnter = "Method_Enter";
        private const string XmlNodeMethodExit = "Method_Exit";
        private const string XmlElementParameter = "Parameter";
        private const string XmlElementElement = "Element";
        private const string XmlAttributeName = "Name";
        private const string XmlAttributeValue = "Value";
        private const string XmlAttributeBinHexValue = "BinHexValue";
        private const string FieldNodeType = "NodeType";
        private const string FieldParamPrefix = "Param_";

        private static readonly XmlReaderSettings StreamingXmlSettings = new()
        {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true
        };

        /// <summary>
        /// Parses XML file using streaming reader, stopping after reading maxNodes Method_Enter/Exit elements.
        /// Much faster for large files as it doesn't load entire DOM into memory.
        /// </summary>
        /// <param name="xmlPath">Path to XML file</param>
        /// <param name="maxNodes">Maximum number of Method_Enter/Exit nodes to read</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parsed method records</returns>
        public async Task<List<MethodRecord>> ParseStreamingAsync(string xmlPath, int maxNodes, CancellationToken cancellationToken = default)
        {
            var records = new List<MethodRecord>(maxNodes);
            int nodesRead = 0;

            await using var stream = File.OpenRead(xmlPath);
            using var reader = XmlReader.Create(stream, StreamingXmlSettings);

            while (await reader.ReadAsync() && nodesRead < maxNodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reader.NodeType == XmlNodeType.Element &&
                    (reader.Name == XmlNodeMethodEnter || reader.Name == XmlNodeMethodExit))
                {
                    var record = ParseMethodNode(reader);
                    if (!reader.IsEmptyElement)
                    {
                        await ParseParametersAsync(reader, record, cancellationToken);
                    }

                    records.Add(BuildMethodRecord(reader.Name, record));
                    nodesRead++;
                }
            }

            return records;
        }

        private static Dictionary<string, string> ParseMethodNode(XmlReader reader)
        {
            var attributes = new Dictionary<string, string>();

            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    attributes[reader.Name] = reader.Value;
                }
                reader.MoveToElement();
            }

            return attributes;
        }

        private static async Task ParseParametersAsync(XmlReader reader, Dictionary<string, string> attributes, CancellationToken cancellationToken)
        {
            var depth = reader.Depth;
            int paramIndex = 1;

            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth)
                    break;

                if (reader.NodeType == XmlNodeType.Element && reader.Name == XmlElementParameter)
                {
                    string paramName = reader.GetAttribute(XmlAttributeName) ?? paramIndex.ToString(CultureInfo.InvariantCulture);
                    string paramValue = "";

                    if (!reader.IsEmptyElement)
                    {
                        var paramDepth = reader.Depth;

                        while (await reader.ReadAsync())
                        {
                            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == paramDepth)
                                break;

                            if (reader.NodeType == XmlNodeType.Element && reader.Name == XmlElementElement)
                            {
                                paramValue = reader.GetAttribute(XmlAttributeValue) ??
                                           reader.GetAttribute(XmlAttributeBinHexValue) ??
                                           "";
                            }
                        }
                    }

                    attributes[$"{FieldParamPrefix}{paramName}"] = paramValue;
                    paramIndex++;
                }
            }
        }

        private static MethodRecord BuildMethodRecord(string nodeName, Dictionary<string, string> attributes)
        {
            return new MethodRecord
            {
                NodeType = nodeName,
                ThreadId = attributes.GetValueOrDefault("ThreadID", ""),
                Address = attributes.GetValueOrDefault("Address", ""),
                Timestamp = attributes.GetValueOrDefault("Timestamp", ""),
                AppName = attributes.GetValueOrDefault("AppName", ""),
                TraceSource = attributes.GetValueOrDefault("TraceSource", ""),
                MethodName = attributes.GetValueOrDefault("MethodName", ""),
                ParamBuf = attributes.GetValueOrDefault("Param_buf", ""),
                ParamVi = attributes.GetValueOrDefault("Param_vi", ""),
                ElapsedNs = attributes.GetValueOrDefault("ElapsedNS", ""),
                AdditionalParameters = attributes
                    .Where(kv => kv.Key.StartsWith(FieldParamPrefix) && kv.Key != "Param_buf" && kv.Key != "Param_vi")
                    .ToDictionary(kv => kv.Key, kv => kv.Value)
            };
        }
    }
}
