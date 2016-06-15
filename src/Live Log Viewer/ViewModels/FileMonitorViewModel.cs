using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;

using LiveLogViewer.FileMonitor;
using LiveLogViewer.Helpers;
using LiveLogViewer.Properties;

namespace LiveLogViewer.ViewModels
{
    public class FileMonitorViewModel : ViewModel, IDisposable
    {
        private readonly ITimedFileMonitor _fileMonitor;
        private bool _fileExists;
        private string _fileName;
        private bool _isFrozen;
        private string _contents;
        private Encoding _encoding;
        private string _encodingName;
        public event Action<FileMonitorViewModel> Renamed;

        public string EncodingName
        {
            get { return _encodingName; }
            set
            {
                if (value == _encodingName) return;
                _encodingName = value;
                _encoding = Encoding.GetEncoding(value);
                _fileMonitor.ChangeEncoding(_encoding);
                OnPropertyChanged();
            }
        }

        protected virtual void OnRenamed()
        {
            Renamed?.Invoke(this);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public FileMonitorViewModel(string filePath, string fileName, string encodingName, bool bufferedRead)
        {
            Preconditions.CheckNotEmptyOrNull(filePath);
            Preconditions.CheckNotEmptyOrNull(fileName);

            FilePath = filePath;
            _fileName = fileName;

            FileExists = File.Exists(filePath);

            try
            {
                _encoding = Encoding.GetEncoding(encodingName);
            }
            catch (Exception)
            {
                MessageBox.Show($"Could not use encoding {encodingName}. Defaulting to UTF8 instead.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _encoding = _encoding ?? Encoding.UTF8;
            _encodingName = _encoding.BodyName;

            _fileMonitor = new TimedFileMonitor(filePath, _encoding, TimeSpan.FromSeconds(Settings.Default.TimerIntervalSeconds)) { BufferedRead = bufferedRead };
            _fileMonitor.FileUpdated += FileMonitorOnFileUpdated;
            _fileMonitor.FileDeleted += FileMonitorOnFileDeleted;
            _fileMonitor.FileCreated += FileMonitorOnFileCreated;
            _fileMonitor.FileRenamed += FileMonitorOnFileRenamed;

            Settings.Default.PropertyChanged += SettingsChanged;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Settings.Default.TimerIntervalSeconds))
                _fileMonitor.TimerInterval = TimeSpan.FromSeconds(Settings.Default.TimerIntervalSeconds);
        }

        private void FileMonitorOnFileRenamed(IFileMonitor fileMonitor, string newPath)
        {
            FilePath = newPath;
            OnRenamed();
        }

        public bool FileExists
        {
            get { return _fileExists; }
            set
            {
                if (value.Equals(_fileExists)) return;
                _fileExists = value;
                OnPropertyChanged();
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value == _fileName) return;
                _fileName = value;
                OnPropertyChanged();
            }
        }

        public string Contents
        {
            get { return _contents; }
            set
            {
                if (value == _contents) return;
                _contents = value;
                OnPropertyChanged();
            }
        }

        public string FilePath { get; private set; }

        public bool IsFrozen
        {
            get { return _isFrozen; }
            set
            {
                if (value.Equals(_isFrozen)) return;
                _isFrozen = value;
                OnPropertyChanged();
            }
        }

        public bool BufferedRead
        {
            get { return _fileMonitor.BufferedRead; }
            set { _fileMonitor.BufferedRead = value; }
        }

        public event Action<FileMonitorViewModel> Updated;

        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void FileMonitorOnFileCreated(IFileMonitor obj)
        {
            OnUpdated();
            FileExists = true;
        }

        private void FileMonitorOnFileDeleted(IFileMonitor obj)
        {
            OnUpdated();
            FileExists = false;
        }

        private void FileMonitorOnFileUpdated(IFileMonitor fileMonitor, string contents)
        {
            OnUpdated();
            Contents += contents;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _fileMonitor?.Dispose();
        }

    }
}