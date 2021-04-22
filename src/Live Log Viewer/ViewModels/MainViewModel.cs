using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;

using LiveLogViewer.Helpers;
using LiveLogViewer.Properties;

namespace LiveLogViewer.ViewModels
{
    public class MainViewModel : ViewModel, IDisposable
    {
        public ObservableCollection<FileMonitorViewModel> FileMonitors { get; } = new ObservableCollection<FileMonitorViewModel>();
        public ObservableCollection<EncodingInfo> AvailableEncodings { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand ClearFileCommand { get; }
        public ICommand FreezeFileCommand { get; }

        public FileMonitorViewModel SelectedFile
        {
            get
            {
                return _selectedFile;
            }
            set
            {
                if (value == _selectedFile) return;
                _selectedFile = value;
                OnPropertyChanged();
            }
        }

        public string LastUpdatedMessage
        {
            get { return _lastUpdated; }
            set
            {
                if (value == _lastUpdated) return;
                _lastUpdated = value;
                OnPropertyChanged();
            }
        }

        public int FontSize => Settings.Default.FontSize;

        public string Font => Settings.Default.Font;

        private readonly Timer _refreshTimer;

        private DateTime? _lastUpdateDateTime;
        private string _lastUpdated;
        private FileMonitorViewModel _lastUpdatedViewModel;
        private FileMonitorViewModel _selectedFile;

        public MainViewModel()
        {
            _refreshTimer = new Timer(OnTimerElapsed);
            _refreshTimer.Change(DateTime.Now.Date.AddDays(1) - DateTime.Now, TimeSpan.FromDays(1));

            AvailableEncodings = new ObservableCollection<EncodingInfo>(Encoding.GetEncodings());

            RemoveFileCommand = new RelayCommand(RemoveFile, CanRemoveFile);
            ClearFileCommand = new RelayCommand(ClearFile, CanClearFile);
            FreezeFileCommand = new RelayCommand(FreezeFile, CanFreezeFile);

            Settings.Default.PropertyChanged += OnSettingsChanged;

            var fileArguments = Environment.GetCommandLineArgs()
                .Skip(1)
                .Where(a => !string.IsNullOrWhiteSpace(a));

            foreach (var argument in fileArguments)
            {
                AddFileMonitor(argument);
            }            
        }

        private void OnSettingsChanged(object s, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(FontSize));
            OnPropertyChanged(nameof(Font));
        }

        public void AddFileMonitor(string filepath)
        {
            foreach (var fileMonitor in FileMonitors)
            {
                if (!fileMonitor.FilePath.Equals(filepath, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                // Already being monitored
                SelectedFile = fileMonitor;
                return;
            }

            var monitorViewModel = new FileMonitorViewModel(filepath, Path.GetFileName(filepath),
                Settings.Default.DefaultEncoding, Settings.Default.BufferedRead);

            monitorViewModel.Renamed += MonitorViewModelOnRenamed;
            monitorViewModel.Updated += MonitorViewModelOnUpdated;

            FileMonitors.Add(monitorViewModel);
            SelectedFile = monitorViewModel;
        }
        private bool CanFreezeFile() => SelectedFile != null;

        private void FreezeFile()
        {
            if (SelectedFile != null)
                SelectedFile.IsFrozen = !SelectedFile.IsFrozen;
        }

        private bool CanClearFile() => !string.IsNullOrEmpty(SelectedFile?.Contents);

        private void ClearFile()
        {
            if (SelectedFile != null)
                SelectedFile.Contents = string.Empty;
        }

        private void RemoveFile()
        {
            if (SelectedFile == null)
                return;

            SelectedFile.Dispose();
            FileMonitors.Remove(SelectedFile);
        }

        private void OnTimerElapsed(object state)
        {
            RefreshLastUpdatedText();
            _refreshTimer.Change(DateTime.Now.Date.AddDays(1) - DateTime.Now, TimeSpan.FromDays(1));
        }

        private void RefreshLastUpdatedText()
        {
            if (_lastUpdateDateTime == null)
                return;

            var dateTime = _lastUpdateDateTime.Value;
            var datestring = dateTime.Date != DateTime.Now.Date ? $" on {dateTime}"
                : $" at {dateTime.ToLongTimeString()}";
            LastUpdatedMessage = _lastUpdatedViewModel.FilePath + datestring;
        }

        private bool CanRemoveFile() => SelectedFile != null;

        private void MonitorViewModelOnUpdated(FileMonitorViewModel fileMonitorViewModel)
        {
            _lastUpdateDateTime = DateTime.Now;
            _lastUpdatedViewModel = fileMonitorViewModel;
            RefreshLastUpdatedText();
        }

        private static void MonitorViewModelOnRenamed(FileMonitorViewModel renamedFileMonitor)
        {
            var filepath = renamedFileMonitor.FilePath;
            renamedFileMonitor.FileName = Path.GetFileName(filepath);
        }

        public void Dispose()
        {
            _refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _refreshTimer.Dispose();
        }
    }
}