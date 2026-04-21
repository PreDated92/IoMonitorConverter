namespace ConverterEngine.Tests;

public class PreviewTests
{
    private readonly string _testDataPath;

    public PreviewTests()
    {
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
        Directory.CreateDirectory(_testDataPath);
    }

    [Fact]
    public async Task GeneratePreviewAsync_ValidXmlFile_ReturnsCorrectCsvContent()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_preview_valid.xml");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi"">
      <Element Value=""123456"" />
    </Parameter>
    <Parameter Name=""buf"">
      <Element BinHexValue=""48656C6C6F"" />
    </Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""100000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.GeneratePreviewAsync(xmlPath);

            Assert.NotEmpty(result);
            Assert.Contains("Timestamp,AppName,Address,TraceSource,MethodName,Param_buf,ReturnValue,Time(ms)", result);
            Assert.Contains("100.000", result);
            Assert.Contains("Hello", result);
            Assert.Contains("VISA", result);
            Assert.Contains("viOpen", result);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
        }
    }

    [Fact]
    public async Task GeneratePreviewAsync_DoesNotCreateFile()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_preview_nofile.xml");
        var possibleOutputPath = Path.Combine(_testDataPath, "test_preview_nofile.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi"">
      <Element Value=""123"" />
    </Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.GeneratePreviewAsync(xmlPath);

            Assert.NotEmpty(result);
            Assert.False(File.Exists(possibleOutputPath), "GeneratePreviewAsync should not create any output file");
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(possibleOutputPath)) File.Delete(possibleOutputPath);
        }
    }

    [Fact]
    public async Task GeneratePreviewAsync_WithMaxRecordsLimit_ReturnsLimitedRecords()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_preview_limit.xml");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
  <Method_Enter Timestamp=""1234567890200000"" ThreadID=""1234"" Address=""0x12346"" TraceSource=""VISA"" MethodName=""viClose"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""456"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890300000"" ThreadID=""1234"" Address=""0x12346"" ElapsedNS=""50000000"" />
  <Method_Enter Timestamp=""1234567890400000"" ThreadID=""1234"" Address=""0x12347"" TraceSource=""VISA"" MethodName=""viRead"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""789"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890500000"" ThreadID=""1234"" Address=""0x12347"" ElapsedNS=""50000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.GeneratePreviewAsync(xmlPath, maxRecords: 2);

            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(3, lines.Length); // Header + 2 data rows
            Assert.Contains("viOpen", result);
            Assert.Contains("viClose", result);
            Assert.DoesNotContain("viRead", result); // Third record should be excluded
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
        }
    }

    [Fact]
    public async Task GeneratePreviewAsync_WithZeroMaxRecords_ReturnsAllRecords()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_preview_all.xml");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
  <Method_Enter Timestamp=""1234567890200000"" ThreadID=""1234"" Address=""0x12346"" TraceSource=""VISA"" MethodName=""viClose"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""456"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890300000"" ThreadID=""1234"" Address=""0x12346"" ElapsedNS=""50000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.GeneratePreviewAsync(xmlPath, maxRecords: 0);

            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(3, lines.Length); // Header + 2 data rows
            Assert.Contains("viOpen", result);
            Assert.Contains("viClose", result);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
        }
    }

    [Fact]
    public async Task GeneratePreviewAsync_WithCancellationToken_ThrowsOperationCanceledException()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_preview_cancel.xml");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await MainEngine.GeneratePreviewAsync(xmlPath, cancellationToken: cts.Token));

            Assert.True(exception is TaskCanceledException or OperationCanceledException);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
        }
    }

    [Fact]
    public async Task GeneratePreviewAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(_testDataPath, "nonexistent_preview.xml");

        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await MainEngine.GeneratePreviewAsync(nonExistentPath));
    }

    [Fact]
    public async Task GeneratePreviewAsync_EmptyXml_ReturnsHeaderOnly()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_preview_empty.xml");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.GeneratePreviewAsync(xmlPath);

            Assert.NotEmpty(result);
            Assert.Contains("Timestamp,AppName,Address,TraceSource,MethodName,Param_buf,ReturnValue,Time(ms)", result);
            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Single(lines); // Only header
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
        }
    }
}
