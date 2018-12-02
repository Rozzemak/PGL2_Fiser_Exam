using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Config;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Interfaces;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.ScheduleManager;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.AllServices;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.View.ViewModel;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Controller
{
    class Controller : IPersistent
    {
        public event EventHandler SaveDataRequest;
        public event EventHandler LoadDataRequest;

        public Config Config;

        public BaseDebug Debug = new BaseDebug();
        public Client Client;

        public StudentService StudentService;
        public ScheduleService ScheduleService;
        public TeacherService TeacherService;

        public AllServices AllServices;

        public ComonScheduleActionManager ComonScheduleActionManager;
        public FillerScheduleActionManager FillerScheduleActionManager;

        public StartUpWindow StartUpWindow;

        public ScheduleView ScheduleView;

        public Controller(StagUser stagUser, ScheduleView scheduleView, MainWindow mainWindow)
        {
            Config = new Config();
 
            this.Client = new Client(Debug, stagUser);
            this.StudentService = new StudentService(Debug, Client);
            this.ScheduleService = new ScheduleService(Debug, Client);
            this.TeacherService = new TeacherService(Debug, Client);

            this.AllServices = new AllServices(StudentService, TeacherService, ScheduleService);

            this.ComonScheduleActionManager = new ComonScheduleActionManager(Debug, AllServices);
            this.FillerScheduleActionManager = new FillerScheduleActionManager(Debug, AllServices);

            if (Config.LoadedStagUser != null)
            {
                Debug.AddMessage<object>(new Message<object>("Automatically logged in,(using stored user).", MessageTypeEnum.Indifferent));
                Client.StagUser = Config.LoadedStagUser;
            }
            else if (stagUser == null) Debug.AddMessage<object>(new Message<object>("Not logged in (yet) => using guest account", MessageTypeEnum.Indifferent));


            this.ScheduleView = scheduleView;
            //InitCustmViews(mainWindow);
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            SaveDataRequest += OnSaveDataRequest;
            LoadDataRequest += OnLoadDataRequest;
        }


        private void InitCustmViews(MainWindow mainWindow)
        {
            StartUpWindow = new StartUpWindow(mainWindow);
        }

        public void OnSaveDataRequest(object sender, EventArgs e)
        {

            SaveDataRequest?.Invoke(this, new PersistentEventArgs(sender));

        }

        public void OnLoadDataRequest(object sender, EventArgs e)
        {

            LoadDataRequest?.Invoke(this, new PersistentEventArgs(sender));

        }

    }
}
