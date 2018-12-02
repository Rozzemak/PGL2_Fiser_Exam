using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Config;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;

namespace PGL2_Fis_Marek_Slavka
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            // Console visibility logic.
            // Hide the debug window, if there is no user config nor the -debug parameter.
            var arguments = Environment.GetCommandLineArgs();
            var config = new Config();
            if (File.Exists(Config.ConfigFileName))
            {
                config.LoadXmlConfig(out StagUser st);
                if (!arguments.Contains("-debug") && !config.ConfigVariables.IsShowDebugConsole)
                {
                    ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 0);
                }
            }
            else
            {
                if (!arguments.Contains("-debug"))
                {
                    ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 0);
                }
            }
        }
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    }
}
