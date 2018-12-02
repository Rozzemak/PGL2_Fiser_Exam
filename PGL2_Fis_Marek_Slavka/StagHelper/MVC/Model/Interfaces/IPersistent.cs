using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Interfaces
{
    public interface IPersistent
    {
        event EventHandler SaveDataRequest;
        event EventHandler LoadDataRequest;
    }

    public class PersistentEventArgs : EventArgs
    {
        public bool IsDataSaved = false;
        public bool IsDataLoaded = false;

        public PersistentEventArgs(object Data)
        {
            if (true)
            {
                LoadData(Data);
                IsDataSaved = true;
            }
            else
            {
                SaveData(Data);
                IsDataLoaded = true;
            }
        }

        private void LoadData(object Container)
        {

        }

        private void SaveData(object Data)
        {

        }
    }
}
