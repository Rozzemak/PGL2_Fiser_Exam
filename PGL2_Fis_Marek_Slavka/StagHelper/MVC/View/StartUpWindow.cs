using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC
{
    class StartUpWindow
    {
        Window _startupWindow;


        public StartUpWindow(MainWindow mainWindow)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _startupWindow = new Window();
                _startupWindow.Owner = mainWindow;
                _startupWindow.Show();
            });
            
        }
    }
}
