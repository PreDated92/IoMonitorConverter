using System.IO;

namespace ConverterEngine;

public class XmlFileInfo
{
    public string FileName { get; }
    public string FullPath { get; }
    public string FileSizeFormatted { get; }
    public string ModifiedDate { get; }

    public XmlFileInfo(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        FileName = fileInfo.Name;
        FullPath = fileInfo.FullName;
        FileSizeFormatted = FormatFileSize(fileInfo.Length);
        ModifiedDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
