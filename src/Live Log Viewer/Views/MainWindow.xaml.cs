using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using LiveLogViewer.Properties;
using LiveLogViewer.ViewModels;

using Microsoft.Win32;

namespace LiveLogViewer.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _mainViewModel;

            ContentRendered += OnContentRendered;
        }

        private void OnContentRendered(object s, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length <= 1)
            {
                PromptForFile();
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            PromptForFile();
        }

        private void PromptForFile()
        {
            var openFileDialog = new OpenFileDialog { CheckFileExists = false, Multiselect = true };

            if (openFileDialog.ShowDialog() != true)
                return;

            try
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    _mainViewModel.AddFileMonitor(fileName);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var fileMonitorViewModel = (FileMonitorViewModel)textBox.DataContext;

            if (fileMonitorViewModel?.IsFrozen == false)
                textBox.ScrollToEnd();
        }

        private void ConfigButton_OnClick(object sender, RoutedEventArgs e)
        {
            new ConfigurationWindow { Owner = this }.ShowDialog();
        }
    }
}