using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using PGL2_Fis_Marek_Slavka.Annotations;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Config
{
    [XmlRoot("ConfigVariables")]
    public class ConfigVariables : INotifyPropertyChanged
    {
        //User_Understandable
        private bool _isRememberUserName = true;
        private bool _isStayLoggedIn = true;
        private bool _isCheckForUpdates = false;


        //Advanced
        private bool _isLoadConfigFromXml = true;
        private bool _isUseCache = true;
        private bool _isShowDebugConsole = false;

        #region GetSetProps

        public bool IsRememberUserName
        {
            get { return _isRememberUserName; }
            set
            {
                if (_isRememberUserName != value)
                {
                    _isRememberUserName = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsStayLoggedIn
        {
            get { return _isStayLoggedIn; }
            set
            {
                if (_isStayLoggedIn != value)
                {
                    _isStayLoggedIn = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsCheckForUpdates
        {
            get { return _isCheckForUpdates; }
            set
            {
                if (_isCheckForUpdates != value)
                {
                    _isCheckForUpdates = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoadConfigFromXml
        {
            get { return _isLoadConfigFromXml; }
            set
            {
                if (_isLoadConfigFromXml != value)
                {
                    _isLoadConfigFromXml = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsUseCache
        {
            get { return _isUseCache; }
            set
            {
                if (_isUseCache != value)
                {
                    _isUseCache = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsShowDebugConsole
        {
            get { return _isShowDebugConsole; }
            set
            {
                if (_isShowDebugConsole != value)
                {
                    _isShowDebugConsole = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        public ConfigStagUser ConfigStagUser;

        public event PropertyChangedEventHandler PropertyChanged;

        public ConfigVariables()
        {

        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class Config
    {
        public ConfigVariables ConfigVariables = new ConfigVariables();

        public const string ConfigFileName = "Config.xml";

        public StagUser LoadedStagUser;

        /// <summary>
        /// If you create config with existing user, it will automatically load itself.
        /// </summary>
        /// <param name="user"></param>
        public Config()
        {
            if (File.Exists(ConfigFileName) && CheckConfigIntegrity())
            {
                LoadXmlConfig(out StagUser stagUser);
                LoadedStagUser = stagUser;
            }
            ConfigVariables.PropertyChanged += ConfigVariables_PropertyChanged;
        }

        private void ConfigVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            new Task(() =>
            {
                if (LoadedStagUser != null)
                    ReplaceXmlConfig(LoadedStagUser);
            }).Start();
        }

        public void CreateAndSaveDefaultXmlConfig(StagUser stagUser, bool append = false)
        {
            XmlSerializer x = new XmlSerializer(typeof(ConfigVariables));
            TextWriter writer = new StreamWriter(ConfigFileName, false);
            ConfigVariables = new ConfigVariables();
            ConfigVariables.ConfigStagUser = new ConfigStagUser(stagUser.UserName, stagUser.Password, stagUser.StagOsId);
            x.Serialize(writer, ConfigVariables);
            writer.Close();
        }

        public void ReplaceXmlConfig(StagUser stagUser)
        {
            XmlSerializer x = new XmlSerializer(typeof(ConfigVariables));
            TextWriter writer = new StreamWriter(ConfigFileName, false);
            x.Serialize(writer, this.ConfigVariables);
            writer.Close();
        }

        public void LoadXmlConfig(out StagUser stagUser)
        {
            if (CheckConfigIntegrity())
            {
                XmlSerializer cvars = new XmlSerializer(typeof(ConfigVariables));
                XmlReader reader = XmlReader.Create(ConfigFileName);
                // Just whatever the exception is, the xml file is damaged, user will be asked to remove it.
                try
                {
                    ConfigVariables = (ConfigVariables)cvars.Deserialize(reader);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Your Config.xml is damaged.\nRemove it.");
                    throw;
                }
                stagUser = new StagUser(ConfigVariables.ConfigStagUser.UserName,
                    ConfigVariables.ConfigStagUser.GetPassword(ConfigVariables.ConfigStagUser.HashedPassword),
                    ConfigVariables.ConfigStagUser.OsId);
            }
            else
            {
                stagUser = null;
                MessageBox.Show("Your Config.xml is damaged.\nRemove it.");
                App.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                });
            }
        }

        public bool CheckConfigIntegrity()
        {
            XmlSerializer x = new XmlSerializer(typeof(ConfigVariables));
            XmlReader reader = XmlReader.Create(ConfigFileName);
            return x.CanDeserialize(reader);
        }

    }
}
