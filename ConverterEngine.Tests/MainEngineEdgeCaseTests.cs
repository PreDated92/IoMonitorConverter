namespace ConverterEngine.Tests;

public class MainEngineEdgeCaseTests
{
    private readonly string _testDataPath;

    public MainEngineEdgeCaseTests()
    {
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
        Directory.CreateDirectory(_testDataPath);
    }

    [Fact]
    public async Task ConvertFile_NonExistentFile_ThrowsException()
    {
        var xmlPath = Path.Combine(_testDataPath, "nonexistent.xml");
        var csvPath = Path.Combine(_testDataPath, "output.csv");

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await MainEngine.ConvertFileAsync(xmlPath, csvPath));
    }

    [Fact]
    public async Task ConvertFile_EmptyXml_GeneratesHeaderOnly()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_empty.xml");
        var csvPath = Path.Combine(_testDataPath, "empty_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
</Events>";

        File.WriteAllText(xmlPath, xmlContent);

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
    public async Task ConvertFile_InvalidHexInParamBuf_HandlesGracefully()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_invalid_hex.xml");
        var csvPath = Path.Combine(_testDataPath, "invalid_hex_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
    <Parameter Name=""buf""><Element BinHexValue=""XYZ"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        File.WriteAllText(xmlPath, xmlContent);

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
    public async Task ConvertFile_MissingElapsedNS_HandlesGracefully()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_no_elapsed.xml");
        var csvPath = Path.Combine(_testDataPath, "no_elapsed_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" />
</Events>";

        File.WriteAllText(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            Assert.NotEmpty(result);
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
    public async Task ConvertFile_MissingAttributes_UsesEmptyStrings()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_missing_attrs.xml");
        var csvPath = Path.Combine(_testDataPath, "missing_attrs_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        File.WriteAllText(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            Assert.NotEmpty(result);
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
    public async Task ConvertFile_ExitBeforeEnter_SkipsBoth()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_exit_first.xml");
        var csvPath = Path.Combine(_testDataPath, "exit_first_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
  </Method_Enter>
</Events>";

        File.WriteAllText(xmlPath, xmlContent);

        try
        {
            var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Single(lines);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }

    [Fact]
    public void ConvertHex_LowerCaseHex_ConvertsCorrectly()
    {
        var hex = "48656c6c6f";
        var result = MainEngine.ConvertHex(hex);
        
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ConvertHex_MixedCaseHex_ConvertsCorrectly()
    {
        var hex = "48656C6c6F";
        var result = MainEngine.ConvertHex(hex);
        
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ConvertHex_NumericOnly_ConvertsCorrectly()
    {
        var hex = "303132333435";
        var result = MainEngine.ConvertHex(hex);
        
        Assert.Equal("012345", result);
    }

    [Fact]
    public void ConvertHex_NewlineCharacter_ConvertsCorrectly()
    {
        var hex = "48656C6C6F0A576F726C64";
        var result = MainEngine.ConvertHex(hex);
        
        Assert.Equal("Hello\nWorld", result);
    }
}
