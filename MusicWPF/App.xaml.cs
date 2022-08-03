using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

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
