using ConverterEngine;

namespace ConverterEngine.Tests;

public class XmlFileInfoTests
{
    [Fact]
    public void Constructor_WithValidFileInfo_SetsPropertiesCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "test content");
        var fileInfo = new FileInfo(tempFile);

        try
        {
            var xmlFileInfo = new XmlFileInfo(fileInfo);

            Assert.Equal(fileInfo.Name, xmlFileInfo.FileName);
            Assert.Equal(fileInfo.FullName, xmlFileInfo.FullPath);
            Assert.NotNull(xmlFileInfo.FileSizeFormatted);
            Assert.NotNull(xmlFileInfo.ModifiedDate);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Constructor_WithNullFileInfo_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new XmlFileInfo(null!));
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(512, "512 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1572864, "1.5 MB")]
    [InlineData(1073741824, "1 GB")]
    public void FileSizeFormatted_VariousSizes_FormatsCorrectly(long bytes, string expected)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            byte[] data = new byte[bytes];
            File.WriteAllBytes(tempFile, data);
            var fileInfo = new FileInfo(tempFile);

            var xmlFileInfo = new XmlFileInfo(fileInfo);

            Assert.Equal(expected, xmlFileInfo.FileSizeFormatted);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ModifiedDate_Format_IsCorrect()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var fileInfo = new FileInfo(tempFile);
            var xmlFileInfo = new XmlFileInfo(fileInfo);

            Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}", xmlFileInfo.ModifiedDate);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Properties_AreReadOnly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var fileInfo = new FileInfo(tempFile);
            var xmlFileInfo = new XmlFileInfo(fileInfo);

            var type = typeof(XmlFileInfo);
            Assert.Null(type.GetProperty("FileName")?.GetSetMethod());
            Assert.Null(type.GetProperty("FullPath")?.GetSetMethod());
            Assert.Null(type.GetProperty("FileSizeFormatted")?.GetSetMethod());
            Assert.Null(type.GetProperty("ModifiedDate")?.GetSetMethod());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
