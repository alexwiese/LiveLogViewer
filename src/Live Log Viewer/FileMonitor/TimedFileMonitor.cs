using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LiveLogViewer
{
    public class TimedFileMonitor : FileMonitor
    {
        private readonly Timer _timer;
        private readonly FileSystemWatcher _watcher;

        public TimedFileMonitor(string filePath, Encoding encoding = null, TimeSpan interval = default(TimeSpan))
            : base(filePath, encoding)
        {
            interval = interval == default(TimeSpan) ? TimeSpan.FromSeconds(2) : interval;

            _timer = new Timer(TimerCallback);
            _timer.Change(interval, interval);

            _watcher = CreateFileWatcher(filePath);
        }

        private FileSystemWatcher CreateFileWatcher(string filePath)
        {
            var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));

            watcher.Changed += WatcherOnChanged;
            watcher.Created += WatcherOnCreated;
            watcher.Deleted += WatcherOnDeleted;
            watcher.Renamed += WatcherOnRenamed;

            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
            watcher.EnableRaisingEvents = true;

            return watcher;
        }

        private async void WatcherOnRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
            FilePath = renamedEventArgs.FullPath;
            _watcher.Filter = renamedEventArgs.Name;

            OnFileRenamed(renamedEventArgs.FullPath);

            await Refresh();
        }

        private async void WatcherOnDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            await Refresh();
        }

        private async void WatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            await Refresh();
        }

        private async void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            await Refresh();
        }

        private async void TimerCallback(object sender)
        {
            await Refresh();
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
                _watcher?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}