using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ConverterEngine;
using Microsoft.Win32;

namespace IoMonitorConverterWpf;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private string _previewText = "Select an XML file to preview the CSV output...";
    private string _statusText = "Ready";
    private string _xmlFilePath = string.Empty;
    private string _csvFilePath = string.Empty;
    private string _currentXmlFolder = "No folder selected";
    private bool _isConverting;
    private bool _isCancellable;
    private bool _isLoadingPreview;
    private CancellationTokenSource? _cancellationTokenSource;
    private ConverterEngine.XmlFileInfo? _selectedXmlFile;
    private readonly ConverterEngine.UserSettings _settings;

    public MainWindowViewModel()
    {
        TextConvertCommand = new RelayCommand(ExecuteTextConvertAsync, () => !IsConverting);
        CancelCommand = new RelayCommand(ExecuteCancel, () => IsCancellable);
        BrowseXmlCommand = new RelayCommand(ExecuteBrowseXml);
        BrowseCsvCommand = new RelayCommand(ExecuteBrowseCsv);

        _settings = ConverterEngine.UserSettings.Load();

        if (!string.IsNullOrEmpty(_settings.LastXmlFolderPath) && Directory.Exists(_settings.LastXmlFolderPath))
        {
            LoadXmlFilesFromFolder(_settings.LastXmlFolderPath);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ConverterEngine.XmlFileInfo> XmlFiles { get; } = [];

    public string PreviewText
    {
        get => _previewText;
        set
        {
            if (_previewText != value)
            {
                _previewText = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText != value)
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
    }

    public string XmlFilePath
    {
        get => _xmlFilePath;
        set
        {
            if (_xmlFilePath != value)
            {
                _xmlFilePath = value;
                OnPropertyChanged();

                if (!string.IsNullOrWhiteSpace(_xmlFilePath) && File.Exists(_xmlFilePath))
                {
                    _ = LoadPreviewAsync(_xmlFilePath);
                }
            }
        }
    }

    public string CsvFilePath
    {
        get => _csvFilePath;
        set
        {
            if (_csvFilePath != value)
            {
                _csvFilePath = value;
                OnPropertyChanged();
            }
        }
    }

    public string CurrentXmlFolder
    {
        get => _currentXmlFolder;
        set
        {
            if (_currentXmlFolder != value)
            {
                _currentXmlFolder = value;
                OnPropertyChanged();
            }
        }
    }

    public ConverterEngine.XmlFileInfo? SelectedXmlFile
    {
        get => _selectedXmlFile;
        set
        {
            if (_selectedXmlFile != value)
            {
                _selectedXmlFile = value;
                OnPropertyChanged();

                if (_selectedXmlFile != null)
                {
                    XmlFilePath = _selectedXmlFile.FullPath;
                }
            }
        }
    }

    public bool IsConverting
    {
        get => _isConverting;
        set
        {
            if (_isConverting != value)
            {
                _isConverting = value;
                OnPropertyChanged();
                ((RelayCommand)TextConvertCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsCancellable
    {
        get => _isCancellable;
        set
        {
            if (_isCancellable != value)
            {
                _isCancellable = value;
                OnPropertyChanged();
                ((RelayCommand)CancelCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsLoadingPreview
    {
        get => _isLoadingPreview;
        set
        {
            if (_isLoadingPreview != value)
            {
                _isLoadingPreview = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand TextConvertCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BrowseXmlCommand { get; }
    public ICommand BrowseCsvCommand { get; }

    private async Task LoadPreviewAsync(string xmlPath)
    {
#if PERF_DIAGNOSTICS
        var uiWatch = System.Diagnostics.Stopwatch.StartNew();
#endif

        IsLoadingPreview = true;
        StatusText = "Loading preview...";

#if PERF_DIAGNOSTICS
        var preStartTime = uiWatch.ElapsedMilliseconds;
        System.Diagnostics.Debug.WriteLine($"[WPF-PERF] UI Setup: {preStartTime}ms");
#endif

        try
        {
#if PERF_DIAGNOSTICS
            var engineStartTime = uiWatch.ElapsedMilliseconds;
#endif
            var previewCsv = await MainEngine.GeneratePreviewAsync(xmlPath, maxRecords: 10);
#if PERF_DIAGNOSTICS
            var engineEndTime = uiWatch.ElapsedMilliseconds;
            System.Diagnostics.Debug.WriteLine($"[WPF-PERF] Engine Call: {engineEndTime - engineStartTime}ms");
#endif

            PreviewText = previewCsv;
#if PERF_DIAGNOSTICS
            var bindingTime = uiWatch.ElapsedMilliseconds;
            System.Diagnostics.Debug.WriteLine($"[WPF-PERF] UI Binding: {bindingTime - engineEndTime}ms");

            StatusText = $"Preview loaded (showing first 10 records) - {uiWatch.ElapsedMilliseconds}ms";
            System.Diagnostics.Debug.WriteLine($"[WPF-PERF] TOTAL UI Time: {uiWatch.ElapsedMilliseconds}ms");
#else
            StatusText = "Preview loaded (showing first 10 records)";
#endif
        }
        catch (Exception ex)
        {
            PreviewText = $"Error loading preview:\n{ex.Message}";
            StatusText = "Preview error";
#if PERF_DIAGNOSTICS
            System.Diagnostics.Debug.WriteLine($"[WPF-PERF] Error at {uiWatch.ElapsedMilliseconds}ms: {ex.Message}");
#endif
        }
        finally
        {
            IsLoadingPreview = false;
        }
    }

    private void LoadXmlFilesFromFolder(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath))
            {
                CurrentXmlFolder = "Folder not found";
                XmlFiles.Clear();
                return;
            }

            var directory = new DirectoryInfo(folderPath);
            var xmlFiles = directory.GetFiles("*.xml");

            XmlFiles.Clear();
            foreach (var file in xmlFiles.OrderByDescending(f => f.LastWriteTime))
            {
                XmlFiles.Add(new ConverterEngine.XmlFileInfo(file));
            }

            CurrentXmlFolder = folderPath;
        }
        catch (Exception ex)
        {
            CurrentXmlFolder = $"Error: {ex.Message}";
            XmlFiles.Clear();
        }
    }

    private void ExecuteBrowseXml()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            Title = "Select XML Input File"
        };

        if (dialog.ShowDialog() == true)
        {
            XmlFilePath = dialog.FileName;

            var folderPath = Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folderPath))
            {
                _settings.LastXmlFolderPath = folderPath;
                _settings.Save();
                LoadXmlFilesFromFolder(folderPath);
            }
        }
    }

    private void ExecuteBrowseCsv()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv",
            Title = "Select CSV Output File"
        };

        if (dialog.ShowDialog() == true)
        {
            CsvFilePath = dialog.FileName;
        }
    }

    private async void ExecuteTextConvertAsync()
    {
        if (string.IsNullOrWhiteSpace(XmlFilePath))
        {
            MessageBox.Show("Please select an XML input file.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(CsvFilePath))
        {
            MessageBox.Show("Please select a CSV output file.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!File.Exists(XmlFilePath))
        {
            MessageBox.Show($"XML file not found:\n{XmlFilePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();

        IsConverting = true;
        IsCancellable = true;

        var stopwatch = Stopwatch.StartNew();
        StatusText = "Converting XML to CSV...";

        try
        {
            await MainEngine.ConvertFileAsync(
                XmlFilePath,
                CsvFilePath,
                cancellationToken: _cancellationTokenSource.Token);

            stopwatch.Stop();
            StatusText = $"Conversion completed in {stopwatch.ElapsedMilliseconds:N0}ms";
            MessageBox.Show(
                $"CSV file generated successfully!\n\nLocation: {CsvFilePath}\nTime: {stopwatch.ElapsedMilliseconds:N0}ms",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            StatusText = "Conversion cancelled by user";
            MessageBox.Show("Conversion was cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (FileNotFoundException ex)
        {
            stopwatch.Stop();
            StatusText = "Error: File not found";
            MessageBox.Show($"File not found:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            StatusText = "Error occurred during conversion";
            MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsConverting = false;
            IsCancellable = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void ExecuteCancel()
    {
        _cancellationTokenSource?.Cancel();
        StatusText = "Cancelling...";
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
