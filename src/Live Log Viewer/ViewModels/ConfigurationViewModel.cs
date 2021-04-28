using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

using LiveLogViewer.Helpers;
using LiveLogViewer.Properties;

namespace LiveLogViewer.ViewModels
{
    public class ConfigurationViewModel : ViewModel
    {
        public ObservableCollection<string> AvailableFonts { get; }
        public ObservableCollection<EncodingInfo> AvailableEncodings { get; }

        public string SelectedFont
        {
            get
            {
                return Settings.Default.Font;
            }
            set
            {
                Settings.Default.Font = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public EncodingInfo SelectedEncoding
        {
            get
            {
                return AvailableEncodings.FirstOrDefault(e => e.Name.Equals(Settings.Default.DefaultEncoding,
                    StringComparison.CurrentCultureIgnoreCase));
            }
            set
            {
                Settings.Default.DefaultEncoding = value.Name;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool BufferedRead
        {
            get
            {
                return Settings.Default.BufferedRead;
            }
            set
            {
                Settings.Default.BufferedRead = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public int TimerInterval
        {
            get
            {
                return Settings.Default.TimerIntervalSeconds;
            }
            set
            {
                Preconditions.CheckArgumentRange(nameof(value), value, 1, int.MaxValue);
                Settings.Default.TimerIntervalSeconds = value;
                Settings.Default.Save(); 
            }
        }

        public int FontSize
        {
            get
            {
                return Settings.Default.FontSize;
            }
            set
            {
                Preconditions.CheckArgumentRange(nameof(value), value, 1, int.MaxValue);
                Settings.Default.FontSize = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public ConfigurationViewModel()
        {
            var fonts = Fonts.SystemFontFamilies
                .Select(f => f.ToString())
                .OrderBy(f => f);

            AvailableFonts = new ObservableCollection<string>(fonts);

            AvailableEncodings = new ObservableCollection<EncodingInfo>(Encoding.GetEncodings());
            SelectedEncoding = AvailableEncodings.FirstOrDefault(e => e.Name.Equals(Settings.Default.DefaultEncoding, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}