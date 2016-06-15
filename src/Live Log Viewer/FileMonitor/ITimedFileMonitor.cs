using System;

namespace LiveLogViewer.FileMonitor
{
    public interface ITimedFileMonitor : IFileMonitor
    {
        TimeSpan TimerInterval { get; set; }
    }
}