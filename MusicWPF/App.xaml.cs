using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MusicWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

            //ConsoleHelper.AllocConsole();
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            
            //Process.Start("MusicWPF.exe");
            //Process.GetCurrentProcess().Kill();
        }
        public class ConsoleHelper
        {
            /// <summary>
            /// Allocates a new console for current process.
            /// </summary>
            [DllImport("kernel32.dll")]
            public static extern Boolean AllocConsole();

            /// <summary>
            /// Frees the console.
            /// </summary>
            [DllImport("kernel32.dll")]
            public static extern Boolean FreeConsole();
        }
    }
}
