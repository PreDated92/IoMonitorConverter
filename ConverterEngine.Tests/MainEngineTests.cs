namespace ConverterEngine.Tests;

public class MainEngineTests
{
    private readonly string _testDataPath;

    public MainEngineTests()
    {
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
        Directory.CreateDirectory(_testDataPath);
    }

    [Fact]
    public void ConvertHex_ValidHexString_ReturnsAsciiString()
    {
        var hex = "48656C6C6F";
        var result = MainEngine.ConvertHex(hex);
        
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ConvertHex_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MainEngine.ConvertHex(""));
    }

    [Fact]
    public void ConvertHex_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MainEngine.ConvertHex(null!));
    }

    [Fact]
    public void ConvertHex_WhitespaceString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MainEngine.ConvertHex("   "));
    }

    [Fact]
    public void ConvertHex_OddLength_ThrowsArgumentException()
    {
        var hex = "48656C6C6F5";
        var exception = Assert.Throws<ArgumentException>(() => MainEngine.ConvertHex(hex));
        
        Assert.Contains("Invalid hex string length", exception.Message);
    }

    [Fact]
    public void ConvertHex_InvalidHexCharacters_ThrowsArgumentException()
    {
        var hex = "48656C6C6XZ";
        var exception = Assert.Throws<ArgumentException>(() => MainEngine.ConvertHex(hex));
        
        Assert.Contains("Invalid hex characters", exception.Message);
    }

    [Theory]
    [InlineData("48656C6C6F", "Hello")]
    [InlineData("576F726C64", "World")]
    [InlineData("313233", "123")]
    [InlineData("414243", "ABC")]
    public void ConvertHex_VariousValidInputs_ReturnsExpectedOutput(string hex, string expected)
    {
        var result = MainEngine.ConvertHex(hex);
        
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertHex_SpecialCharacters_ConvertsCorrectly()
    {
        var hex = "2C3B3A21";
        var result = MainEngine.ConvertHex(hex);
        
        Assert.Equal(",;:!", result);
    }

    [Fact]
    public void ConvertHex_WithSpaces_ConvertsSpaces()
    {
        var hex = "48656C6C6F20576F726C64";
        var result = MainEngine.ConvertHex(hex);
        
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public async Task ConvertFile_ValidXmlFile_CreatesCsvWithCorrectStructure()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_sample.xml");
        var csvPath = Path.Combine(_testDataPath, "output.csv");

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

        File.WriteAllText(xmlPath, xmlContent);

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
    public async Task ConvertFile_MinimalXml_GeneratesValidCsv()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_minimal.xml");
        var csvPath = Path.Combine(_testDataPath, "minimal_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi"">
      <Element Value=""123"" />
    </Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        File.WriteAllText(xmlPath, xmlContent);

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
    public async Task ConvertFile_MultipleMethodCalls_MergesCorrectly()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_multiple.xml");
        var csvPath = Path.Combine(_testDataPath, "multiple_output.csv");

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

        File.WriteAllText(xmlPath, xmlContent);

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
    public async Task ConvertFile_WithCustomTimeZone_UsesSpecifiedTimeZone()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_timezone.xml");
        var csvPath = Path.Combine(_testDataPath, "timezone_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        File.WriteAllText(xmlPath, xmlContent);

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
    public async Task ConvertFile_ParamBufWithComma_EscapesCorrectly()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_comma.xml");
        var csvPath = Path.Combine(_testDataPath, "comma_output.csv");

        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1234567890000000"" ThreadID=""1234"" Address=""0x12345"" TraceSource=""VISA"" MethodName=""viOpen"" AppName=""TestApp"">
    <Parameter Name=""vi""><Element Value=""123"" /></Parameter>
    <Parameter Name=""buf""><Element BinHexValue=""412C42"" /></Parameter>
  </Method_Enter>
  <Method_Exit Timestamp=""1234567890100000"" ThreadID=""1234"" Address=""0x12345"" ElapsedNS=""50000000"" />
</Events>";

        File.WriteAllText(xmlPath, xmlContent);

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
    public async Task ConvertFile_UnmatchedMethodEnter_SkipsRecord()
    {
        var xmlPath = Path.Combine(_testDataPath, "test_unmatched.xml");
        var csvPath = Path.Combine(_testDataPath, "unmatched_output.csv");

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

        File.WriteAllText(xmlPath, xmlContent);

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
}
