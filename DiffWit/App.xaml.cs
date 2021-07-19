using DiffWit.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DiffWit
{
    public partial class App : Application
    {
        private Window _currentWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _currentWindow = new MainWindow();
            _currentWindow.Activate();
        }

        //protected override void OnActivated(IActivatedEventArgs args)
        //{
        //    switch (args.Kind)
        //    {
        //        case ActivationKind.CommandLineLaunch:
        //            {
        //                var cmdLineArgs = args as CommandLineActivatedEventArgs;
        //                var operation = cmdLineArgs.Operation;
        //                HandleCommandLineActivation(operation.Arguments, operation.CurrentDirectoryPath);
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void HandleCommandLineActivation(string arguments, string currentDirectory)
        //{
        //    Frame rootFrame = Window.Current.Content as Frame;
        //    if (rootFrame == null)
        //    {
        //        rootFrame = new Frame();
        //        Window.Current.Content = rootFrame;
        //    }
        //
        //    var parsedCommands = CommandLineParser.ParseUntrustedArgs(arguments);
        //    if (parsedCommands != null && parsedCommands.Count > 0)
        //    {
        //        rootFrame.Navigate(typeof(MainPage), parsedCommands);
        //    }
        //    else
        //    {
        //        rootFrame.Navigate(typeof(MainPage));
        //    }
        //
        //    Window.Current.Activate();
        //}
    }
}
