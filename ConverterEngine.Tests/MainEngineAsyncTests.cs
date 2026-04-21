namespace ConverterEngine.Tests;

public class MainEngineAsyncTests
{
    private readonly string _testDataPath;

    public MainEngineAsyncTests()
    {
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
        Directory.CreateDirectory(_testDataPath);
    }

    [Fact]
    public async Task ConvertFileAsync_ValidXmlFile_CreatesCsvWithCorrectStructure()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_sample.xml");
        var csvPath = Path.Combine(_testDataPath, "async_output.csv");

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
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            Assert.True(File.Exists(csvPath), "CSV file should be created");
            Assert.NotEmpty(result);
            Assert.Contains("Timestamp,AppName,Address,TraceSource,MethodName,Param_buf,ReturnValue,Time(ms)", result);
            Assert.Contains("100.000", result);
            Assert.Contains("Hello", result);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_MinimalXml_GeneratesValidCsv()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_minimal.xml");
        var csvPath = Path.Combine(_testDataPath, "async_minimal_output.csv");

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
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            Assert.NotEmpty(result);
            Assert.Contains("50.000", result);
            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, lines.Length);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_MultipleMethodCalls_MergesCorrectly()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_multiple.xml");
        var csvPath = Path.Combine(_testDataPath, "async_multiple_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1000000000"" ThreadID=""1"" Address=""0x1"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""App1"">
    <Parameter Name=""vi""><Element Value=""100"" /></Parameter>
    <Parameter Name=""buf""><Element BinHexValue=""414141"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1000100000"" ThreadID=""1"" Address=""0x1"" ElapsedNS=""10000000"" />
  
  <Method_Enter Timestamp=""2000000000"" ThreadID=""2"" Address=""0x2"" TraceSource=""VISA"" MethodName=""viRead"" AppName=""App2"">
    <Parameter Name=""vi""><Element Value=""200"" /></Parameter>
    <Parameter Name=""buf""><Element BinHexValue=""424242"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""2000200000"" ThreadID=""2"" Address=""0x2"" ElapsedNS=""20000000"">
    <Parameter Name=""buf""><Element BinHexValue=""434343"" /></Parameter>
  </Method_Exit>
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(3, lines.Length);
            Assert.Contains("10.000", result);
            Assert.Contains("20.000", result);
            Assert.Contains("CCC", result);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_WithCustomTimeZone_UsesSpecifiedTimeZone()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_timezone.xml");
        var csvPath = Path.Combine(_testDataPath, "async_timezone_output.csv");

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
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath, "UTC");

            Assert.NotEmpty(result);
            Assert.Contains("UTC", TimeZoneInfo.Utc.Id);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_ParamBufWithComma_EscapesCorrectly()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_comma.xml");
        var csvPath = Path.Combine(_testDataPath, "async_comma_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
    <Parameter Name=""buf""><Element BinHexValue=""412C42"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            Assert.Contains("\"A,B\"", result);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_cancel.xml");
        var csvPath = Path.Combine(_testDataPath, "async_cancel_output.csv");

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

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await MainEngine.ConvertFileAsync(xmlPath, csvPath, cancellationToken: cts.Token);
            });
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        var xmlPath = Path.Combine(_testDataPath, "nonexistent_async.xml");
        var csvPath = Path.Combine(_testDataPath, "async_nonexistent_output.csv");

        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await MainEngine.ConvertFileAsync(xmlPath, csvPath);
        });
    }

    [Fact]
    public async Task ConvertFileAsync_EmptyXml_GeneratesHeaderOnly()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_empty.xml");
        var csvPath = Path.Combine(_testDataPath, "async_empty_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Single(lines);
            Assert.Contains("Timestamp,AppName,Address,TraceSource,MethodName,Param_buf,ReturnValue,Time(ms)", result);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_InvalidHexInParamBuf_HandlesGracefully()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_invalid_hex.xml");
        var csvPath = Path.Combine(_testDataPath, "async_invalid_hex_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
    <Parameter Name=""buf""><Element BinHexValue=""XYZ"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            Assert.NotEmpty(result);
            Assert.DoesNotContain("XYZ", result);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_UnmatchedMethodEnter_SkipsRecord()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_async_unmatched.xml");
        var csvPath = Path.Combine(_testDataPath, "async_unmatched_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1000000000"" ThreadID=""1"" Address=""0x1"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""App1"">
    <Parameter Name=""vi""><Element Value=""100"" /></Parameter>
  </Method_Enter>
  <Method_Enter Timestamp=""2000000000"" ThreadID=""2"" Address=""0x2"" TraceSource=""VISA"" MethodName=""viRead"" AppName=""App2"">
    <Parameter Name=""vi""><Element Value=""200"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""2000200000"" ThreadID=""2"" Address=""0x2"" ElapsedNS=""20000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, lines.Length);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ConvertFileAsync_ConcurrentCalls_BothSucceed()
    {
        var xmlPath1 = Path.Combine(_testDataPath, "test_async_concurrent1.xml");
        var csvPath1 = Path.Combine(_testDataPath, "async_concurrent_output1.csv");
        var xmlPath2 = Path.Combine(_testDataPath, "test_async_concurrent2.xml");
        var csvPath2 = Path.Combine(_testDataPath, "async_concurrent_output2.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        await File.WriteAllTextAsync(xmlPath1, xmlContent);
        await File.WriteAllTextAsync(xmlPath2, xmlContent);

        try
        {
            var task1 = MainEngine.ConvertFileAsync(xmlPath1, csvPath1);
            var task2 = MainEngine.ConvertFileAsync(xmlPath2, csvPath2);

            var results = await Task.WhenAll(task1, task2);

            Assert.All(results, result => Assert.NotEmpty(result));
            Assert.True(File.Exists(csvPath1));
            Assert.True(File.Exists(csvPath2));
        }
        finally
        {
            if (File.Exists(xmlPath1)) File.Delete(xmlPath1);
            if (File.Exists(csvPath1)) File.Delete(csvPath1);
            if (File.Exists(xmlPath2)) File.Delete(xmlPath2);
            if (File.Exists(csvPath2)) File.Delete(csvPath2);
        }
    }
}
