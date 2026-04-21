using ConverterEngine;
using System.Diagnostics;

namespace IoMonitorConverterForm
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly ConverterEngine.UserSettings _settings;

        public Form1()
        {
            InitializeComponent();

            _settings = ConverterEngine.UserSettings.Load();

            if (!string.IsNullOrEmpty(_settings.LastXmlFolderPath) && Directory.Exists(_settings.LastXmlFolderPath))
            {
                LoadXmlFilesFromFolder(_settings.LastXmlFolderPath);
            }
        }

        private void LoadXmlFilesFromFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    lblCurrentFolder.Text = "Folder not found";
                    lvXmlFiles.Items.Clear();
                    return;
                }

                var directory = new DirectoryInfo(folderPath);
                var xmlFiles = directory.GetFiles("*.xml");

                lvXmlFiles.Items.Clear();
                foreach (var file in xmlFiles.OrderByDescending(f => f.LastWriteTime))
                {
                    var fileInfo = new ConverterEngine.XmlFileInfo(file);
                    var item = new ListViewItem(fileInfo.FileName);
                    item.SubItems.Add(fileInfo.FileSizeFormatted);
                    item.SubItems.Add(fileInfo.ModifiedDate);
                    item.Tag = fileInfo.FullPath;
                    lvXmlFiles.Items.Add(item);
                }

                lblCurrentFolder.Text = folderPath;
            }
            catch (Exception ex)
            {
                lblCurrentFolder.Text = $"Error: {ex.Message}";
                lvXmlFiles.Items.Clear();
            }
        }

        private async void lvXmlFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lvXmlFiles.SelectedItems.Count > 0)
            {
                var selectedItem = lvXmlFiles.SelectedItems[0];
                txtXmlFilePath.Text = selectedItem.Tag?.ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(txtXmlFilePath.Text))
                {
                    await LoadPreviewAsync(txtXmlFilePath.Text);
                }
            }
        }

        private async Task LoadPreviewAsync(string xmlPath)
        {
#if PERF_DIAGNOSTICS
            var uiWatch = System.Diagnostics.Stopwatch.StartNew();
#endif

            progressBarPreview.Visible = true;
            progressBarPreview.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "Loading preview...";

#if PERF_DIAGNOSTICS
            var preStartTime = uiWatch.ElapsedMilliseconds;
            System.Diagnostics.Debug.WriteLine($"[WinForms-PERF] UI Setup: {preStartTime}ms");
#endif

            try
            {
#if PERF_DIAGNOSTICS
                var engineStartTime = uiWatch.ElapsedMilliseconds;
#endif
                var previewCsv = await MainEngine.GeneratePreviewAsync(xmlPath, maxRecords: 10);
#if PERF_DIAGNOSTICS
                var engineEndTime = uiWatch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine($"[WinForms-PERF] Engine Call: {engineEndTime - engineStartTime}ms");
#endif

                rtbPreview.Text = previewCsv;
#if PERF_DIAGNOSTICS
                var bindingTime = uiWatch.ElapsedMilliseconds;
                System.Diagnostics.Debug.WriteLine($"[WinForms-PERF] UI Update: {bindingTime - engineEndTime}ms");

                lblStatus.Text = $"Preview loaded (showing first 10 records) - {uiWatch.ElapsedMilliseconds}ms";
                System.Diagnostics.Debug.WriteLine($"[WinForms-PERF] TOTAL UI Time: {uiWatch.ElapsedMilliseconds}ms");
#else
                lblStatus.Text = "Preview loaded (showing first 10 records)";
#endif
            }
            catch (Exception ex)
            {
                rtbPreview.Text = $"Error loading preview:\n{ex.Message}";
                lblStatus.Text = "Preview error";
#if PERF_DIAGNOSTICS
                System.Diagnostics.Debug.WriteLine($"[WinForms-PERF] Error at {uiWatch.ElapsedMilliseconds}ms: {ex.Message}");
#endif
            }
            finally
            {
                progressBarPreview.Visible = false;
            }
        }

        private async void btBrowseXml_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select XML Input File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtXmlFilePath.Text = dialog.FileName;

                var folderPath = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    _settings.LastXmlFolderPath = folderPath;
                    _settings.Save();
                    LoadXmlFilesFromFolder(folderPath);
                }

                await LoadPreviewAsync(dialog.FileName);
            }
        }

        private void btBrowseCsv_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                Title = "Select CSV Output File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtCsvFilePath.Text = dialog.FileName;
            }
        }

        private async void btTextConvert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtXmlFilePath.Text))
            {
                MessageBox.Show("Please select an XML input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCsvFilePath.Text))
            {
                MessageBox.Show("Please select a CSV output file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(txtXmlFilePath.Text))
            {
                MessageBox.Show($"XML file not found:\n{txtXmlFilePath.Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            btTextConvert.Enabled = false;
            btCancel.Enabled = true;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            var stopwatch = Stopwatch.StartNew();
            lblStatus.Text = "Converting XML to CSV...";

            try
            {
                await MainEngine.ConvertFileAsync(
                    txtXmlFilePath.Text,
                    txtCsvFilePath.Text,
                    cancellationToken: _cancellationTokenSource.Token);

                stopwatch.Stop();
                lblStatus.Text = $"Conversion completed in {stopwatch.ElapsedMilliseconds:N0}ms";
                MessageBox.Show(
                    $"CSV file generated successfully!\n\nLocation: {txtCsvFilePath.Text}\nTime: {stopwatch.ElapsedMilliseconds:N0}ms",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                lblStatus.Text = "Conversion cancelled by user";
                MessageBox.Show("Conversion was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (FileNotFoundException ex)
            {
                stopwatch.Stop();
                lblStatus.Text = "Error: File not found";
                MessageBox.Show($"File not found:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                lblStatus.Text = "Error occurred during conversion";
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btTextConvert.Enabled = true;
                btCancel.Enabled = false;
                progressBar.Visible = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            lblStatus.Text = "Cancelling...";
        }
    }
}
