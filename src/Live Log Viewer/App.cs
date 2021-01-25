using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace LiveLogViewer
{
    public class App : Application
    {
        public App()
        {
            Startup += App_OnStartup;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App
            {
                StartupUri = new Uri("Views/MainWindow.xaml", UriKind.Relative)
            };
            app.Run();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            OnUnhandledException(unhandledExceptionEventArgs.ExceptionObject as Exception);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            OnUnhandledException(dispatcherUnhandledExceptionEventArgs.Exception);
        }

        private void OnUnhandledException(Exception exception)
        {
            MessageBox.Show(MainWindow, $"Error: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}