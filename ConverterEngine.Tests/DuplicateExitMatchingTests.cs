using Xunit;

namespace ConverterEngine.Tests
{
    /// <summary>
    /// Tests for the duplicate Method_Exit matching bug fix.
    /// Verifies that each Method_Exit is only matched once, even when multiple Method_Enter
    /// records share the same ThreadID and Address.
    /// </summary>
    public class DuplicateExitMatchingTests
    {
        [Fact]
        public async Task ConvertFileAsync_DuplicateThreadIdAndAddress_DoesNotReuseSameExit()
        {
            // Arrange
            var xmlPath = Path.Combine(Path.GetTempPath(), $"test_duplicate_{Guid.NewGuid()}.xml");
            var csvPath = Path.Combine(Path.GetTempPath(), $"test_duplicate_output_{Guid.NewGuid()}.csv");

            // Create XML with 2 Method_Enter records with SAME ThreadID and Address,
            // but only 1 Method_Exit
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1000000000"" ThreadID=""1234"" Address=""0x100"" AppName=""TestApp"" TraceSource=""VISA"" MethodName=""viOpen"" />
  <Method_Enter Timestamp=""2000000000"" ThreadID=""1234"" Address=""0x100"" AppName=""TestApp"" TraceSource=""VISA"" MethodName=""viOpen"" />
  <Method_Exit Timestamp=""3000000000"" ThreadID=""1234"" Address=""0x100"" ElapsedNS=""100000000"" />
</Events>";

            try
            {
                File.WriteAllText(xmlPath, xmlContent);

                // Act
                var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

                // Assert
                var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Should have header + only 1 merged record (not 2)
                // First Enter matches the Exit, second Enter has no matching unused Exit
                Assert.Equal(2, lines.Length); // Header + 1 data row

                // Verify the CSV content
                Assert.Contains("Timestamp,AppName,Address,TraceSource,MethodName,Param_buf,ReturnValue,Time(ms)", lines[0]);

                // Only the first Method_Enter should be matched
                var dataLine = lines[1];
                Assert.Contains("TestApp", dataLine);
                Assert.Contains("0x100", dataLine);
                Assert.Contains("viOpen", dataLine);
                Assert.Contains("100.000", dataLine); // Elapsed time from the single Exit
            }
            finally
            {
                if (File.Exists(xmlPath)) File.Delete(xmlPath);
                if (File.Exists(csvPath)) File.Delete(csvPath);
            }
        }

        [Fact]
        public async Task ConvertFileAsync_MultipleEnterExitPairs_MatchesCorrectly()
        {
            // Arrange
            var xmlPath = Path.Combine(Path.GetTempPath(), $"test_multiple_pairs_{Guid.NewGuid()}.xml");
            var csvPath = Path.Combine(Path.GetTempPath(), $"test_multiple_pairs_output_{Guid.NewGuid()}.csv");

            // Create XML with 2 Enter records with same Thread+Address,
            // and 2 corresponding Exit records
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1000000000"" ThreadID=""1234"" Address=""0x100"" AppName=""TestApp"" TraceSource=""VISA"" MethodName=""viOpen"" />
  <Method_Exit Timestamp=""2000000000"" ThreadID=""1234"" Address=""0x100"" ElapsedNS=""50000000"" />
  <Method_Enter Timestamp=""3000000000"" ThreadID=""1234"" Address=""0x100"" AppName=""TestApp"" TraceSource=""VISA"" MethodName=""viOpen"" />
  <Method_Exit Timestamp=""4000000000"" ThreadID=""1234"" Address=""0x100"" ElapsedNS=""75000000"" />
</Events>";

            try
            {
                File.WriteAllText(xmlPath, xmlContent);

                // Act
                var result = await MainEngine.ConvertFileAsync(xmlPath, csvPath);

                // Assert
                var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Should have header + 2 merged records (each Enter paired with its own Exit)
                Assert.Equal(3, lines.Length); // Header + 2 data rows

                // First pair should have 50ms elapsed time
                Assert.Contains("50.000", lines[1]);

                // Second pair should have 75ms elapsed time
                Assert.Contains("75.000", lines[2]);

                // Verify both pairs are different (not duplicates)
                Assert.NotEqual(lines[1], lines[2]);
            }
            finally
            {
                if (File.Exists(xmlPath)) File.Delete(xmlPath);
                if (File.Exists(csvPath)) File.Delete(csvPath);
            }
        }

        [Fact]
        public async Task GeneratePreviewAsync_DuplicateThreadIdAndAddress_DoesNotReuseSameExit()
        {
            // Arrange
            var xmlPath = Path.Combine(Path.GetTempPath(), $"test_preview_duplicate_{Guid.NewGuid()}.xml");

            // Create XML with 2 Method_Enter records with SAME ThreadID and Address,
            // but only 1 Method_Exit
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1000000000"" ThreadID=""1234"" Address=""0x100"" AppName=""TestApp"" TraceSource=""VISA"" MethodName=""viOpen"" />
  <Method_Enter Timestamp=""2000000000"" ThreadID=""1234"" Address=""0x100"" AppName=""TestApp"" TraceSource=""VISA"" MethodName=""viOpen"" />
  <Method_Exit Timestamp=""3000000000"" ThreadID=""1234"" Address=""0x100"" ElapsedNS=""100000000"" />
</Events>";

            try
            {
                File.WriteAllText(xmlPath, xmlContent);

                // Act
                var result = await MainEngine.GeneratePreviewAsync(xmlPath, maxRecords: 10);

                // Assert
                var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Should have header + only 1 merged record (not 2)
                Assert.Equal(2, lines.Length); // Header + 1 data row

                // Verify the CSV content
                Assert.Contains("Timestamp,AppName,Address,TraceSource,MethodName,Param_buf,ReturnValue,Time(ms)", lines[0]);
                Assert.Contains("100.000", lines[1]); // Elapsed time from the single Exit
            }
            finally
            {
                if (File.Exists(xmlPath)) File.Delete(xmlPath);
            }
        }

        [Fact]
        public async Task GeneratePreviewAsync_MultipleEnterExitPairs_MatchesCorrectly()
        {
            // Arrange
            var xmlPath = Path.Combine(Path.GetTempPath(), $"test_preview_multiple_{Guid.NewGuid()}.xml");

            // Create XML with 2 Enter records with same Thread+Address,
            // and 2 corresponding Exit records
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Events>
  <Method_Enter Timestamp=""1000000000"" ThreadID=""1234"" Address=""0x100"" AppName=""TestApp"" TraceSource=""VISA"" MethodName=""viOpen"" />
  <Method_Exit Timestamp=""2000000000"" ThreadID=""1234"" Address=""0x100"" ElapsedNS=""50000000"" />
  <Method_Enter Timestamp=""3000000000"" ThreadID=""1234"" Address=""0x100"" AppName=""TestApp"" TraceSource=""VISA"" MethodName=""viOpen"" />
  <Method_Exit Timestamp=""4000000000"" ThreadID=""1234"" Address=""0x100"" ElapsedNS=""75000000"" />
</Events>";

            try
            {
                File.WriteAllText(xmlPath, xmlContent);

                // Act
                var result = await MainEngine.GeneratePreviewAsync(xmlPath, maxRecords: 10);

                // Assert
                var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Should have header + 2 merged records (each Enter paired with its own Exit)
                Assert.Equal(3, lines.Length); // Header + 2 data rows

                // First pair should have 50ms elapsed time
                Assert.Contains("50.000", lines[1]);

                // Second pair should have 75ms elapsed time
                Assert.Contains("75.000", lines[2]);
            }
            finally
            {
                if (File.Exists(xmlPath)) File.Delete(xmlPath);
            }
        }
    }
}
