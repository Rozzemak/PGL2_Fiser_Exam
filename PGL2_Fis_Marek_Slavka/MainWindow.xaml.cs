using System;
using System.CodeDom;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Controller;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Threading;
using System.Xml;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Config;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Enums;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.WeekRange;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.View.ViewModel;
using RoutedEventArgs = System.Windows.RoutedEventArgs;
using System.Net;

namespace PGL2_Fis_Marek_Slavka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread _thread;
        private Controller _controller;
        private ScheduleView _scheduleView;

        private SelectionFilter _selectionFilter;
        private const string AddFilterDefaultContent = "Add New Filter";

        #region WindowsInit
        public MainWindow()
        {
            var arguments = Environment.GetCommandLineArgs();
            if (arguments.Contains("-debug"))
            {

                Console.WriteLine("Loading, please wait.");
                new Thread(() => CheckWindow()).Start();
            }
            else
            {
                // Practically hide cmd.
                // Console hide was moved to app.xaml.cs, because it´s quicker init.     
            }

            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += OnClosed;
        }

        private void CheckWindow()
        {
            bool loaded = false;
            bool stop = false;
            Thread th = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(25);
                    Console.Write("█");
                    if (stop)
                    {
                        break;
                    }
                }
            });
            th.Start();
            while (true)
            {
                Thread.Sleep(1);
                this.Dispatcher.Invoke(() =>
                {
                    if ((Application.Current.MainWindow).IsLoaded) loaded = true;
                });
                if (loaded)
                {
                    stop = true;
                    break;
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _scheduleView = new ScheduleView(ScheduleRootGrid, DateWeekStackPanel, DateDayStackPanel,
                FilterStackPanel, ScheduleContentStackPanel, new ScheduleContentModel());
            // usage: (STAG:username, STAG:password)
            _controller = new Controller(new StagUser("guest", new SecureString()), _scheduleView, this);
            _controller.Debug.AddMessage<object>(new Message<object>("Loaded!", MessageTypeEnum.Indifferent));
            _controller.Debug.AddMessage<object>(new Message<object>("Usage: Log in with your stUsername, stPassword"));
            SettingsStackPanel.MouseDown += delegate
            {
                if (Mouse.RightButton == MouseButtonState.Pressed)
                {
                    // It's the right button.
                    // Drag with right_click will fire exception.
                }
                else
                {
                    // It's the standard left button.
                    DragMove();
                }

            };
            StartUpGrid.Visibility = Visibility.Visible;
            LoggedInGrid.Visibility = Visibility.Collapsed;
            _cardFoundStudent = CardFoundStudent;
            this.KeyDown += MainWindow_KeyDown;
            // This sub is little bit ambiguous, so keep this in mind.
            StagUserNameTextBox.KeyDown += StagPasswordTextBox_OnKeyDown;
            GenerateWeekStackPanel();
            SetWeekDateScrollViewSrollbarProgress();
            if (Environment.GetCommandLineArgs().Contains("-debug"))
            {
                SetConsolePosition((int)this.Left, (int)(this.Top + 20));
            }
        }
        #endregion

        #region ExitBehavior
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (!IsDialogShown())
                {
                    if (ExitSelectionGrid.Visibility == Visibility.Collapsed)
                    {
                        if (ExitGrid.Visibility != Visibility.Visible)
                            ExitSelectionGrid.Visibility = Visibility.Visible;
                    }
                    else
                        ExitApp();
                }
                else
                {
                    CloseAllDialogs();
                }
            }
        }

        private void ExitButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            ExitApp();
        }

        private void ExitButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ExitSelectionGrid.Visibility = Visibility.Collapsed;
        }

        private void ExitApp()
        {
            new Task(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    StartUpGrid.Visibility = Visibility.Collapsed;
                    StartUpWelcomeGrid.Visibility = Visibility.Collapsed;
                    StartUpGridWrapperGrid.Visibility = Visibility.Collapsed;
                    ShadowDimmGridGrid.Visibility = Visibility.Collapsed;
                    ExitSelectionGrid.Visibility = Visibility.Collapsed;
                    ExitGrid.Visibility = Visibility.Visible;
                });
                bool stopThread = false;
                new Task(() =>
                {
                    while (!stopThread)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            LoggedInGrid.Visibility = Visibility.Collapsed;
                        });
                        Thread.Sleep(30);
                    }
                }).Start();
                Thread.Sleep(5000);
                stopThread = true;
                App.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                });
            }).Start();
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            Environment.Exit(0);
        }
        #endregion

        #region Tests
        /// <summary>
        /// Controller test..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Impl interface for json dao retrievable objects, example, teacher,schedule..
            // https://ws.ujep.cz/ws/services/
            // MyOsId="F17026"
            // DK="F17198"
            // Dkop = "F17017"
            // Ďurďi = "F17009"
            _thread = new Thread(() =>
            {
                // Init all wished students to compare.
                List<Student> students = new List<Student>()
                {
                    // Máme stejný obor, prolnutí předmětů bude vysoké
                    // --
                    _controller.StudentService.GetStudentByOsId("F17026"),
                    _controller.StudentService.GetStudentByOsId("F17017"),
                    _controller.StudentService.GetStudentByOsId("F17009"),
                    // --
                    // Kamarád, na jiném oboru, pěkně odfiltruje spol. předměty.
                    //_controller.StudentService.GetStudentByOsId("F17198")

                };

                // WriteDown their common scheduleActions.
                _controller.ComonScheduleActionManager.GetCommonScheduleActionsByStudents(students);
                // (or filter them addionally by starting absolute date.. example down below)
                // _controller.ComonScheduleActionManager.GetUncommonScheduleActionsByStudents(students, DateTime.Today);

                // Filter common scheduleActions by teacher.
                _controller.ComonScheduleActionManager.WriteAllCommonScheduledActionsByStudents(students);
                _controller.ScheduleService.WriteScheduleActionsInfo(
                    _controller.ComonScheduleActionManager.FilterScheduleActionsByByTeacher(
                        students, _controller.TeacherService.GetTeacherBySurnameAndName("Krátká", "Magdalena")));

                // Hit controller test few more times, should be able to see, that things in fact do cache.
                // No time limit for cache, so if something changes, it will not update.
            });
            _thread.Start();
        }

        /// <summary>
        /// Proof of concept.
        /// Should have My own view class, but time is dire. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindCommonScheduleActions_Click(object sender, RoutedEventArgs e)
        {
            new Task(() =>
            {
                List<Student> students = new List<Student>();
                Teacher teacher;
                string[] studentStrArr = new[] { "" };
                string teacherStr = "";
                //Cannot read ui vars from another thread.
                Dispatcher.Invoke(() =>
                {
                    studentStrArr = StudentAndTeacherTextBox.Text.Substring("students".Length + 2,
                        StudentAndTeacherTextBox.Text.IndexOf("}", StringComparison.Ordinal)).Split(',');
                    teacherStr =
                        StudentAndTeacherTextBox.Text.Substring(StudentAndTeacherTextBox.Text.IndexOf("Teacher", StringComparison.Ordinal));
                    teacherStr = teacherStr.Replace("Teacher", "");
                    teacherStr = teacherStr.Substring(1, teacherStr.Length - 2);
                });

                foreach (var s in studentStrArr)
                {

                    if (s.Contains("F") && !s.Contains("}"))
                    {
                        students.Add(_controller.StudentService.GetStudentByOsId(s));
                    }
                    else
                    {
                        students.Add(_controller.StudentService.GetStudentByOsId(s.Substring(0, s.Length - 1)));
                        break;
                    }
                }
                teacher = _controller.TeacherService.GetTeacherBySurnameAndName(teacherStr.Split(',')[0].Trim(),
                    teacherStr.Split(',')[1].Trim());
                _controller.ComonScheduleActionManager.GetCommonScheduleActionsByStudents(students);
                if (!teacher.Equals(default(Teacher)))
                {
                    _controller.ScheduleService.WriteScheduleActionsInfo(_controller.ComonScheduleActionManager.FilterScheduleActionsByByTeacher(students, teacher));
                }
                else
                {
                    _controller.ComonScheduleActionManager.WriteAllCommonScheduledActionsByStudents(students);
                }
            }).Start();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new Task(() =>
            {
                var students = new List<Student>()
                {
                    _controller.AllServices.StudentService.GetStudentByOsId("F17026"),
                    _controller.AllServices.StudentService.GetStudentByOsId("F17198")
                };
                _controller.FillerScheduleActionManager.GetUncommonScheduleActionsByStudents(
                    students, DateTime.Parse("Feb 1, 2018"));

                _controller.FillerScheduleActionManager.WriteAllCommonScheduledActionsByStudents(students);
            }).Start();
        }
        #endregion

        #region StartUpGridLogic
        private void StartUpLoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            //StartUpWelcomeGrid.Visibility = Visibility.Hidden;
            new Task((() =>
            {
                Thread.Sleep(400);
                App.Current.Dispatcher.Invoke(() =>
                {
                    StartUpLogInDialogPanel.Visibility = Visibility.Visible;
                    if (_controller.Config.LoadedStagUser != null && _controller.Config.ConfigVariables.IsRememberUserName)
                    {
                        StagUserNameTextBox.Text = _controller.Client.StagUser.UserName;
                        StagPasswordTextBox.Password = "filler";
                    }
                    else
                    {
                        StagUserNameTextBox.Text = "";
                        StagPasswordTextBox.Password = "";
                    }
                });

                if (_controller.Config.LoadedStagUser != null && _controller.Config.ConfigVariables.IsRememberUserName
                    && _controller.Config.ConfigVariables.IsStayLoggedIn)
                {
                    string name = _controller.Config.LoadedStagUser.UserName;
                    SecureString psswd = _controller.Config.LoadedStagUser.Password;
                    _controller = new Controller(new StagUser(name, psswd), _scheduleView, this);
                    _controller.Client.SetLoginData(_controller.Client.StagUser);
                    AttemtToLogIn();
                }
                else if (_controller.Config.LoadedStagUser != null && _controller.Config.ConfigVariables.IsRememberUserName)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        StagPasswordTextBox.Password = "";
                    });
                }
            })).Start();
        }

        private void StagPasswordTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter == e.Key)
            {
                _thread = new Thread(() =>
                {
                    string name = "";
                    string passwd = "";
                    SecureString scPasswd = new SecureString();
                    //Initialises strings from UI thread, cause, if your thread throws exception, it will die. (errorstate -1 or so..)
                    //Because of reinit of whole controller, which was created to fit this aproach, it´s best to reinit whole controller,
                    //but the controller cannot run in same thread as UI, that would be stu***. 
                    //Also you have to use dispatcher for accesing ui thread, or another thread variables, probly cause of mutex lock.
                    //So dont forget to use it. (all of this is self-reminder).. ignore me :-D
                    Dispatcher.Invoke(() =>
                    {
                        name = StagUserNameTextBox.Text; passwd = StagPasswordTextBox.Password;
                        LogInProgressBar.Visibility = Visibility.Visible;
                        LogInStatusLabel.Visibility = Visibility.Collapsed;
                    });
                    //Populate secure string from my password. Then erasing non hashed password asap.
                    foreach (var c in passwd.ToCharArray())
                    {
                        scPasswd.AppendChar(c);
                    }
                    passwd = "";
                    _controller = new Controller(new StagUser(name, scPasswd), _scheduleView, this);
                    _controller.Client.SetLoginData(_controller.Client.StagUser);
                    AttemtToLogIn();
                });
                _thread.Start();
            }
        }

        private void AttemtToLogIn()
        {
            if (!string.IsNullOrEmpty(_controller.Client.StagUser.StagOsId))
            {
                _controller.Debug.AddMessage<object>(new Message<object>("Login credentials set from UI (controller re-init), Unverified."));
                _controller.Debug.AddMessage<object>(new Message<object>("Testing login credentials."));
                Dispatcher.Invoke(() =>
                {
                    LogInStatusLabel.Visibility = Visibility.Visible;
                    LogInStatusLabel.Background = Brushes.Honeydew;
                    LogInStatusLabel.Content = "Checking account credentials";
                    LogInProgressBar.Visibility = Visibility.Hidden;
                });
                Thread.Sleep(400);
                if (_controller.ScheduleService
                        .LoadScheduleByStudent(
                            _controller.StudentService.GetStudentByOsId(_controller.Client.StagUser.StagOsId)) == null)
                {

                    Dispatcher.Invoke(() =>
                    {
                        LogInStatusLabel.Visibility = Visibility.Visible;
                        LogInStatusLabel.Background = Brushes.PaleVioletRed;
                        LogInStatusLabel.Content = "Login credentials are incorrect!";
                    });
                }
                else
                {
                    Student student = _controller.StudentService.GetStudentByOsId(_controller.Client.StagUser.StagOsId);
                    Dispatcher.Invoke(() =>
                    {
                        LogInStatusLabel.Visibility = Visibility.Visible;
                        LogInStatusLabel.Background = Brushes.MediumSeaGreen;
                        LogInStatusLabel.Content = "Sucess!";
                        var button = CloneUIElement(FilterAddButton_0) as Button;
                        SetUIElementContentWithStudent(button, student);
                        button.Click += FilterAddButton_Click;
                        FilterStackPanel.Children.Insert(0, button);
                        SetWeekDateScrollViewSrollbarProgress();
                    });
                    _controller.Debug.AddMessage<object>(
                        new Message<object>("Login credentials are ok! Login process finished."));
                    Thread.Sleep(400);
                    Dispatcher.Invoke(() =>
                    {
                        StartUpWelcomeGrid.Visibility = Visibility.Collapsed;
                        StartUpGrid.Visibility = Visibility.Collapsed;
                        StartUpGridWrapperGrid.Visibility = Visibility.Collapsed;
                        LoggedInGrid.Visibility = Visibility.Visible;
                        SettingsExpander.Header = SettingsExpander.Header + " - [" + student.Name + "] [" + student.Surname + "] "
                                                  + "[" + _controller.Client.StagUser.StagOsId + "]";
                    });
                    //Check for xml deserialization, then if it´s possible, do not replace config, just load it.
                    //File exist is self explanatory.
                    if (!File.Exists(Config.ConfigFileName) || _controller.Config.CheckConfigIntegrity() == false)
                        _controller.Config.CreateAndSaveDefaultXmlConfig(_controller.Client.StagUser);
                    else
                    {
                        //_controller.Config.LoadXmlConfig();
                    }
                }
            }
        }
        #endregion

        #region ScheduleLogicRegion
        #region FilterButtonLogic 
        private Button _lastClickedFilterButton = new Button();
        private void FilterAddButton_Click(object sender, RoutedEventArgs e)
        {
            _lastClickedFilterButton = sender as Button;
            string osId = "";
            if ((sender as Button).Content.ToString().Split(',').Length > 1 && (sender as Button).Content.ToString() != AddFilterDefaultContent)
                osId = (sender as Button).Content.ToString().Split(',')[2];
            if (osId == "")
                if (FilterCard.Visibility != Visibility.Visible)
                {
                    FilterCard.Visibility = Visibility.Visible;
                    ShadowDimmGridGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    // Interesting ?
                }
            else
            {
                FillAndShowCard(osId);
            }
        }

        private void FilterAddConfirmationButton_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = CloneUIElement(_lastClickedFilterButton) as Button;

            CardFilterAddConfirmationButton.Visibility = Visibility.Collapsed;

            if ((button is null))
            {
                // I don´t wanna throw exceptions in UI thread.
                // throw new Exception("You did not fill fields properly.");
                _controller.Debug.AddMessage<object>(new Message<object>("You did not click on any filter button."));
            }
            else
            {
                if (!_controller.AllServices.StudentService.GetStudentByOsId(CardOsIdTextBox.Text).Equals(default(Student)))
                    if (FilterStackPanel.Children.Count <
                        (((FilterStackPanel.ActualHeight / (button.Height + button.Margin.Top))) - 1))
                    {
                        CloseAllDialogs();
                        Student student = _controller.AllServices.StudentService.GetStudentByOsId(CardOsIdTextBox.Text);
                        button.Name += FilterStackPanel.Children.Count;
                        FilterStackPanel.Children.Add(button);
                        button.Click += FilterAddButton_Click;
                        SetUIElementContentWithStudent(_lastClickedFilterButton, student);
                        ClearFilterCard();
                        UpdateStudentSelection(new List<Student>());
                        CardFilterFoundStudentsCard.Visibility = Visibility.Collapsed;
                        ShadowDimmGridGrid.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //Not gonna happen
                    }
                else
                {
                    CardFilterHelpPanel.Content = "No student fetched.";
                    CardFilterHelpPanel.Background = Brushes.Crimson;
                }
            }

        }
        #endregion //FilterButtonLogic
        #region Card
        private Student _lastFetchStudentByOsIdAttempt = default(Student);
        private List<Student> _lastFetchedStudentsByNameAndSurenameAttempt = new List<Student>();
        private void FilterCard_OnKeyDown(object sender, KeyEventArgs e)
        {
            _lastFetchStudentByOsIdAttempt = default(Student);
            _lastFetchedStudentsByNameAndSurenameAttempt = new List<Student>();
            _controller.Config.ConfigVariables.IsCheckForUpdates = true;
            if (e.Key == Key.Enter)
            {
                new Task(() =>
                {
                    string osId = "";
                    string name = "";
                    string surename = "";
                    bool isNameQueue = true;

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        osId = CardOsIdTextBox.Text;
                        name = CardNameTextBox.Text;
                        surename = CardSureNameTextBox.Text;
                        if (surename == "" && name == "") isNameQueue = false;
                        if (surename == "") surename = "%";
                        if (name == "") name = "%";

                        CardFilterHelpPanel.Background = Brushes.BurlyWood;
                        CardFilterHelpPanel.Content = "Qeurying data";
                    });
                    if (osId != "")
                        _lastFetchStudentByOsIdAttempt = _controller.AllServices.StudentService.GetStudentByOsId(osId);
                    else if (isNameQueue)
                        _lastFetchedStudentsByNameAndSurenameAttempt =
                            _controller.AllServices.StudentService.LoadStudentsByNameAndSureName(name, surename);
                    if (!_lastFetchStudentByOsIdAttempt.Equals(default(Student)))
                    {
                        // Student found.
                        new Thread(() =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                CardFilterHelpPanel.Background = Brushes.MediumSeaGreen;
                                CardFilterHelpPanel.Content = "Student found, card updated!";
                                CardNameSpecTextbox.Text = "[" + _lastFetchStudentByOsIdAttempt.Name + "," + _lastFetchStudentByOsIdAttempt.Surname + "]";
                                CardRoleTextBox.Text = "{" + _lastFetchStudentByOsIdAttempt.GetType().Name + "}";
                                CardFilterAddConfirmationButton.Visibility = Visibility.Visible;
                            });
                        }).Start();
                    }
                    else
                    if (!isNameQueue && osId != null)
                    {
                        // Student not found.
                        new Thread(() =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                CardFilterHelpPanel.Background = Brushes.Crimson;
                                CardFilterHelpPanel.Content = "No spec. Student found!";
                                CardFilterFoundStudentsCard.Visibility = Visibility.Collapsed;
                                UpdateStudentSelection(new List<Student>());
                                CardFilterAddConfirmationButton.Visibility = Visibility.Collapsed;
                            });
                        }).Start();
                    }
                    else
                    if (_lastFetchedStudentsByNameAndSurenameAttempt.Count > 0 && _lastFetchedStudentsByNameAndSurenameAttempt.Last().OsId != null)
                    {
                        new Thread(() =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                CardFilterHelpPanel.Background = Brushes.MediumSeaGreen;
                                CardFilterHelpPanel.Content = "Students found, select one!";
                                CardFilterFoundStudentsCard.Visibility = Visibility.Visible;
                                UpdateStudentSelection(_lastFetchedStudentsByNameAndSurenameAttempt);
                            });
                        }).Start();
                    }
                    else
                    {
                        // Student not found.
                        new Thread(() =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                CardFilterHelpPanel.Background = Brushes.Crimson;
                                CardFilterHelpPanel.Content = "No spec. Student found!";
                                CardFilterFoundStudentsCard.Visibility = Visibility.Collapsed;
                                CardFilterAddConfirmationButton.Visibility = Visibility.Collapsed;
                                UpdateStudentSelection(new List<Student>());
                            });
                        }).Start();
                    }
                    Thread.Sleep(2000);
                    //_controller.AllServices.TeacherService.GetTeacherBySurnameAndName(OsIdTextBox.Text) != default(Teacher)
                }).Start();
            }
        }

        // This card gets init in window_loaded; because of static ref. (name)
        private Card _cardFoundStudent = new Card();
        private void UpdateStudentSelection(List<Student> foundStudents)
        {
            CardFilterFoundStudentsPlaceHolder.Children.Clear();
            // Create copy of uiElement using xaml reader, did not found better solution.
            // This is quickest way to ensure, that every field is as it is supposed to be.
            foreach (var student in foundStudents)
            {
                Card _card = CloneUIElement(_cardFoundStudent) as Card;
                _card.Visibility = Visibility.Visible;
                _card.Name += CardFilterFoundStudentsPlaceHolder.Children.Count;
                foreach (var stackChild in (((_card.Content as StackPanel).Children[0]) as DockPanel).Children)
                {
                    if (stackChild is Label)
                    {
                        (stackChild as Label).Name += CardFilterFoundStudentsPlaceHolder.Children.Count;
                        (stackChild as Label).Content = student.Name + "," + student.Surname + "," + student.OsId;
                        _card.MouseDown += delegate
                        {
                            FillAndShowCard(student.OsId);
                            new Task(() =>
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                    { (stackChild as Label).Background = Brushes.MediumSeaGreen; });
                                Thread.Sleep(400);
                                App.Current.Dispatcher.Invoke(() =>
                                { (stackChild as Label).Background = Brushes.WhiteSmoke; });
                            }).Start();
                            CardFilterHelpPanel.Content = "Student selected! Card updated.";
                            CardFilterHelpPanel.Background = Brushes.MediumSeaGreen;
                            // Simulate enter press, for on_keydown event to trigger;
                            InputManager.Current.ProcessInput(
                                new KeyEventArgs(Keyboard.PrimaryDevice,
                                    Keyboard.PrimaryDevice.ActiveSource,
                                    0, Key.Enter)
                                {
                                    RoutedEvent = Keyboard.KeyDownEvent
                                }
                            );
                            CardFilterAddConfirmationButton.Visibility = Visibility.Visible;
                        };
                    }
                }
                CardFilterFoundStudentsPlaceHolder.Children.Add(_card);
            }
        }

        private void DeleteCardFilterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_lastClickedFilterButton.Content as string != AddFilterDefaultContent)
                if (FilterStackPanel.Children.Count > 1)
                {
                    FilterStackPanel.Children.Remove(_lastClickedFilterButton);
                }
            CloseAllDialogs();
            ClearFilterCard();
        }

        private void ClearFilterCard(bool visibility = false)
        {
            if (!visibility)
                FilterCard.Visibility = Visibility.Collapsed;

            CardOsIdTextBox.Text = "";
            CardNameTextBox.Text = "";
            CardSureNameTextBox.Text = "";

            CardFilterHelpPanel.Background = Brushes.WhiteSmoke;
            CardFilterHelpPanel.Content = "*Only colored part is req.*";

            CardNameSpecTextbox.Text = "Name, Surename";
            CardRoleTextBox.Text = "Role: {Student, Učitel, Katedra, Administrator}";

            _lastFetchStudentByOsIdAttempt = default(Student);
        }

        private void CloseCardFilterButton_OnClick(object sender, RoutedEventArgs e)
        {
            CloseAllDialogs();
            ClearFilterCard();
        }

        private void FillAndShowCard(string osId)
        {
            new Task(() =>
            {
                Student student = _controller.StudentService.GetStudentByOsId(osId);
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (!student.Equals(default(Student)))
                    {
                        FilterCard.Visibility = Visibility.Visible;
                        ShadowDimmGridGrid.Visibility = Visibility.Visible;
                        CardOsIdTextBox.Text = student.OsId;
                        CardNameTextBox.Text = student.Name;
                        CardSureNameTextBox.Text = student.Surname;

                        CardFilterHelpPanel.Background = Brushes.WhiteSmoke;
                        CardFilterHelpPanel.Content = "*Only colored part is req.*";

                        CardNameSpecTextbox.Text = "[" + student.Name + "," + student.Surname + "]";
                        CardRoleTextBox.Text = "{" + student.GetType().Name + "}";
                    }
                });
            }).Start();
        }

        private void StudentCardDebugButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetConsolePosition((int)this.Left, (int)(this.Top + 20));
            new Task(() =>
            {
                string osId = "";
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (CardOsIdTextBox.Text != "")
                        osId = CardOsIdTextBox.Text;
                });
                _controller.StudentService.WriteStudentInfo(_controller.StudentService.GetStudentByOsId(osId));
            }).Start();
        }
        #endregion // Card
        #region ScheduleGenerationLogic
        #region ScheduleContentGeneartionLogic
        private void GenerateSchedule(WeekRange weekRange, Button button)
        {
            // IF there is concurence in schedule actions we need to add every sAction time and then filter concurent. (leave fist)
            List<ScheduleAction> concurentTime = new List<ScheduleAction>();
            List<ScheduleAction> freeTime = new List<ScheduleAction>();
            string semester = "";
            if (weekRange.MM < 9) semester = "LS";
            else semester = "ZS";
            // If there are less than 2 people (+ default button) then we don´t need to filter them.
            if (FilterStackPanel.Children.Count < 3 && (FilterStackPanel.Children[0] as Button).Content as string != AddFilterDefaultContent)
            {
                FilterSelectionSlideBar_OnValueChanged(this, new RoutedPropertyChangedEventArgs<double>((double)_selectionFilter, (double)SelectionFilter.None));
                foreach (var scheduleAction in _controller.ScheduleService
                    .GetScheduleByStudent(
                        _controller.AllServices.StudentService.GetStudentByOsId((FilterStackPanel.Children[0] as Button)
                            .Content.ToString().Split(',')[2])).ScheduleJson.ScheduleActions)
                {
                    // Use only day set schedule actions, because it´s schedule. Not DB. 
                    FitScheduleActionsToSchedule(scheduleAction, semester, concurentTime, freeTime);
                }
                //PopulateScheduleFreeTime(semester);
            }
            else if (FilterStackPanel.Children.Count > 2)
            {
                if(_selectionFilter == SelectionFilter.None)
                FilterSelectionSlideBar_OnValueChanged(this, new RoutedPropertyChangedEventArgs<double>((double)_selectionFilter, (double)SelectionFilter.Common));
                List<Student> students = new List<Student>();
                List<ScheduleAction> scheduleActions = new List<ScheduleAction>();
                foreach (var studentButton in FilterStackPanel.Children)
                {
                    if ((studentButton as Button).Content.ToString() != AddFilterDefaultContent)
                    {
                        Student student = _controller.StudentService.GetStudentByOsId((studentButton as Button).Content.ToString().Split(',')[2]);
                        if (!students.Contains(student))
                            students.Add(student);
                    }
                }
                switch (_selectionFilter)
                {
                    case SelectionFilter.Common:
                        var task1 =  new Task(() =>
                        {
                            App.Current.Dispatcher.Invoke(() => { DateWeekStackPanel.Visibility = Visibility.Collapsed; });
                            _controller.ComonScheduleActionManager.GetCommonScheduleActionsByStudents(students)
                                .TryGetValue(students, out scheduleActions);
                            foreach (var scheduleAction in scheduleActions)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    FitScheduleActionsToSchedule(scheduleAction, semester, concurentTime, freeTime);
                                });
                            }
                            App.Current.Dispatcher.Invoke(() => { DateWeekStackPanel.Visibility = Visibility.Visible; });
                        });
                        task1.Start();
                        new Task(() =>
                        {
                            while (!task1.IsCompleted && !task1.IsFaulted)
                            {
                                Thread.Sleep(30);
                            }
                            App.Current.Dispatcher.Invoke(() => { DateWeekStackPanel.Visibility = Visibility.Visible; });
                        }).Start();
                        break;
                    case SelectionFilter.FreeTime:
                        var task2 = new Task(() =>
                        {
                            App.Current.Dispatcher.Invoke(() => { DateWeekStackPanel.Visibility = Visibility.Collapsed; });
                            _controller.ComonScheduleActionManager.GetCommonScheduleActionsByStudents(students)
                                .TryGetValue(students, out scheduleActions);
                            foreach (var scheduleAction in scheduleActions)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    FitScheduleActionsToSchedule(scheduleAction, semester, concurentTime, freeTime);
                                });
                            }
                        });
                        task2.Start();
                        new Task(() =>
                        {
                            while (!task2.IsCompleted && !task2.IsFaulted)
                            {
                                Thread.Sleep(30);
                            }
                            App.Current.Dispatcher.Invoke(() => { DateWeekStackPanel.Visibility = Visibility.Visible; });
                        }).Start();
                        break;
                    case SelectionFilter.None:
                        // Done above.
                        break;
                }
            }
        }

        private void FitScheduleActionsToSchedule(ScheduleAction scheduleAction, string semester, List<ScheduleAction> concurentTime, List<ScheduleAction> freeTime)
        {
            if (scheduleAction.Day != null)
            {
                foreach (var dockPChild in DateDayStackPanel.Children)
                {
                    foreach (var dayButton in (dockPChild as DockPanel).Children)
                    {
                        // Translate scheduleaction day of week wia enum reflexion. Then get int representation of that day.
                        int dayOfweek = (int)Enum.Parse(typeof(DayTranslateCs), scheduleAction.Day);
                        if (dayOfweek == (int)Enum.Parse(typeof(DayTranslateEn),
                                (dayButton as Button).Content.ToString().Substring(0, 3)) &&
                            (scheduleAction.LectureType == "Přednáška" ||
                             scheduleAction.LectureType == "Cvičení" ||
                             scheduleAction.LectureType == "Seminář" ||
                            scheduleAction.LectureType == "SpolVolno") &&
                            //Filters concurent schedule actions in 1 day. (start or end between intestect time on same day) null check of course.
                            scheduleAction.Semester == semester && !concurentTime.Exists(action =>
                                scheduleAction.HourRelativeFrom != null && scheduleAction.HourRelativeTo != null
                                                                        && action.HourRelativeFrom != null &&
                                                                        action.HourRelativeTo != null
                                                                        && int.Parse(action.HourRelativeFrom) <=
                                                                        int.Parse(scheduleAction.HourRelativeTo)
                                                                        && int.Parse(action.HourRelativeTo) >=
                                                                        int.Parse(scheduleAction.HourRelativeTo)
                                                                        && action.Day == scheduleAction.Day))
                        {
                            // If there is alreay concurent action in collection, above if statement wont let concurent action trough.
                            PopulateScheduleFreeTime(freeTime, scheduleAction, dayOfweek);
                            concurentTime.Add(scheduleAction);
                            freeTime.Add(scheduleAction);
                            CreateScheduleContentButton(scheduleAction, dayOfweek);
                        }
                    }
                }
            }
        }

        private void CreateScheduleContentButton(ScheduleAction scheduleAction, int dayOfWeek)
        {
            Button scheduleContentButton = new Button()
            {
                Padding = new Thickness(5, 0, 5, 0),
                Content = scheduleAction.Subject,
                FontSize = 11,
                Width = Math.Abs((545 / 15) * scheduleAction.GetScheduleActionHourRelativeLength()),
                MaxWidth = Math.Abs((545 / 15) * scheduleAction.GetScheduleActionHourRelativeLength()),
                Background = GetBrushType(scheduleAction),
                BorderBrush = GetBrushType(scheduleAction),
                Foreground = Brushes.DarkSlateBlue,
            };
            if (_selectionFilter == SelectionFilter.FreeTime && scheduleContentButton.Content.ToString() != "Break")
            {
                scheduleContentButton.Visibility = Visibility.Hidden;
            } else if (_selectionFilter == SelectionFilter.FreeTime)
            {
                scheduleContentButton.Background = Brushes.BlueViolet;
            }
            scheduleContentButton.Click += delegate (object sender, RoutedEventArgs args)
            {
                ScheduleContentButton_Click(sender, args, scheduleAction);
            };

            if ((ScheduleContentStackPanel.Children[dayOfWeek] as DockPanel).Children.Count == 0)
            {
                Button scheduleFillerButton = new Button()
                {
                    Padding = new Thickness(5, 0, 5, 0),
                    Width = Math.Abs((545 / 15) * (scheduleAction.GetRelativeSchoolHourFromAbsolute())),
                    MaxWidth = Math.Abs((545 / 15) * (scheduleAction.GetRelativeSchoolHourFromAbsolute())),
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Foreground = Brushes.DarkSlateBlue,
                };
                (ScheduleContentStackPanel.Children[dayOfWeek] as DockPanel).Children.Add(scheduleFillerButton);
            }
            (ScheduleContentStackPanel.Children[dayOfWeek] as DockPanel).Children.Add(scheduleContentButton);
        }

        private void ScheduleContentButton_Click(object sender, RoutedEventArgs e, ScheduleAction scheduleAction)
        {
            _controller.AllServices.ScheduleService.WriteScheduleActionInfo(scheduleAction);
        }

        private void PopulateScheduleFreeTime(List<ScheduleAction> freeTime, ScheduleAction scheduleAction, int dayOfWeek)
        {
            if (freeTime.Count > 0)
            {
                foreach (var FreTimeAction in freeTime)
                {
                    if (FreTimeAction.Day == scheduleAction.Day &&
                        scheduleAction.HourAbsoluteFrom.Value.GetDateTime() >
                        FreTimeAction.HourAbsoluteTo.Value.GetDateTime())
                    {
                        ScheduleAction fillableScheduleAction = new ScheduleAction()
                        {
                            Name = "[" + FreTimeAction.Subject + "]_To_[" +
                                   scheduleAction.Subject + "]",
                            ScheduleActionIdno = FreTimeAction.ScheduleActionIdno + "_free",
                            HourAbsoluteFrom = FreTimeAction.HourAbsoluteTo.Value,
                            HourAbsoluteTo = scheduleAction.HourAbsoluteFrom.Value,
                            Day = scheduleAction.Day,
                            Subject = "Break",
                            LectureType = "Volno"
                        };
                        CreateScheduleContentButton(fillableScheduleAction, dayOfWeek);
                        break;
                    }
                }
                freeTime.Clear();
            }
        }

        private Brush GetBrushType(ScheduleAction scheduleAction)
        {
            switch (scheduleAction.LectureType)
            {
                case "Přednáška":
                    return Brushes.LightGray;
                case "Cvičení":
                    return Brushes.MediumSeaGreen;
                case "Seminář":
                    return Brushes.DarkCyan;
                case "Volno":
                    return Brushes.Transparent;
                case "SpolVolno":
                    return Brushes.MediumSeaGreen;
            }
            return Brushes.DarkSlateBlue;
        }

        private void ClearScheduleContent()
        {
            foreach (var dPanel in ScheduleContentStackPanel.Children)
            {
                (dPanel as DockPanel).Children.Clear();
            }
        }
        #endregion //ScheduleContentGenerationLogic
        #region DateGenerationLogic
        private Button _dateWeekActualButton = new Button();
        readonly List<int> partialWeeks = new List<int>();
        private void GenerateWeekStackPanel()
        {
            // Todo: think of a way to specify start/end of school year.
            var year = DateTime.Now.Year;
            var wr = WeekRanger.GetWeekRange(new DateTime(year, 01, 01), new DateTime(year, 12, 31));

            // If there is week, that has been split because it ends after current(week) month,
            // lets keep it´s number to eliminate duplicate week , that would be even more than 7 days longer xD.
            int partialWeek = 0;

            foreach (var weekRange in wr)
            {
                // WeekRange class fetched from some https://www.codeproject.com/Questions/875741/Get-Date-Range-by-month-and-week-number-csharp
                // learn it a bit. Would not hurt me to learn proper Date usage, as for IEnumerable type, seems better than list for this stuff.
                // Had to adjust start of the week (culture specific) -> monday, and introduce temp. var to keep partial weeks. Maybe not ideal.

                // If it is whole week -> (delta day is >= 6)
                if ((weekRange.End - weekRange.Start).TotalDays == 6)
                {
                    GenerateWeekDateButton(weekRange);
                    _dateWeekActualButton.Content = weekRange.Start.Date.Day + "." + weekRange.Start.Month + "-"
                                                    + weekRange.End.Date.Day + "." + weekRange.End.Month;
                }
                // If week is shorter (month ended and week is still not finished)
                // Lets grab WeekRange from [element at + 1] pos to fetch upcoming week and compare them.
                // Also, check for +1 out of bounds hase to be implemented, thats why we are checking last element.
                else
                // We need that date from upcoming month (week end) and current month (week start).. nothing else here.
                if (!wr.Last().Equals(weekRange) && (weekRange.End - weekRange.Start).TotalDays < 6 && weekRange.WeekNo - 1 != partialWeek)
                {
                    partialWeek = weekRange.WeekNo;
                    partialWeeks.Add(partialWeek);
                    GenerateWeekDateButton(weekRange);
                    _dateWeekActualButton.Content = weekRange.Start.Date.Day + "." + weekRange.Start.Month + "-"
                                                    + ((wr.ElementAt(weekRange.WeekNo + 1) as WeekRange?).Value.Start.Date.Day - 1) + "." +
                                                    (wr.ElementAt(weekRange.WeekNo + 1) as WeekRange?).Value.Start.Month;
                }
                // If we are iterating trough last element, just use it.
                else if (wr.Last().Equals(weekRange))
                {
                    GenerateWeekDateButton(weekRange);
                    _dateWeekActualButton.Content = weekRange.Start.Date.Day + "." + weekRange.Start.Month + "-"
                                                    + weekRange.End.Date.Day + "." + weekRange.End.Month;
                }
            }
        }

        /// <summary>
        /// Just generates button and adds it to parent chilrden collection.
        /// </summary>
        /// <param name="weekRange"></param>
        private void GenerateWeekDateButton(WeekRange weekRange)
        {
            _dateWeekActualButton = CloneUIElement(DateWeekActualButton) as Button;
            _dateWeekActualButton.Name += weekRange.MM;
            _dateWeekActualButton.Visibility = Visibility.Visible;
            DateWeekStackPanel.Children.Add(_dateWeekActualButton);
            _dateWeekActualButton.Click += delegate (object sender, RoutedEventArgs args)
            {
                DateWeekActualButtonOnClick(sender, args, weekRange);
            };
        }

        private void DateWeekActualButtonOnClick(object sender, RoutedEventArgs routedEventArgs, WeekRange weekRange)
        {
            ClearDateDayStackPanel(weekRange);
            (sender as Button).Background = Brushes.DarkSlateBlue;
            (sender as Button).BorderBrush = Brushes.DarkSlateBlue;
            foreach (var child in DateWeekStackPanel.Children)
            {
                if (child != sender)
                {
                    (child as Button).Background = DateWeekActualButton.Background;
                    (child as Button).BorderBrush = DateWeekActualButton.BorderBrush;
                }
            }
            PopulateDateDayStackPanel(weekRange);
            ClearScheduleContent();
            GenerateSchedule(weekRange, sender as Button);
        }

        // When DateWeekButton is clicked, for user friendliness, add date to days.
        void PopulateDateDayStackPanel(WeekRange weekRange)
        {
            // Populate the DateDayStackPanel. Seemed not necesarry, but looks nice.
            for (int i = 0; i < 7; i++)
            {
                ((DateDayStackPanel.Children[i] as DockPanel).Children[0] as Button).Content =
                    weekRange.Start.Date.AddDays(i).Date.DayOfWeek.ToString().Substring(0, 3) + "\n" +
                    weekRange.Start.Date.AddDays(i).Date.Day;
            }
        }

        //If DateWeekButton is clicked, other day buttons content needs to be cleared.
        private void ClearDateDayStackPanel(WeekRange weekRange)
        {
            if (!partialWeeks.Contains(weekRange.WeekNo))
                foreach (var sChild in DateDayStackPanel.Children)
                {
                    (((sChild as DockPanel).Children)[0] as Button).Content = "";
                }
        }

        private void SetWeekDateScrollViewSrollbarProgress()
        {
            int childNum = 0;
            var date = DateTime.Now;
            foreach (var sChild in DateWeekStackPanel.Children)
            {
                //If we iterate trough first child, its the reference button used for cloning without correct format.
                if (!DateWeekStackPanel.Children[0].Equals(sChild))
                {
                    ClearScheduleContent();
                    var buttonContent = (sChild as Button).Content.ToString().Split('-');
                    int day = Byte.Parse(buttonContent[0].Split('.')[0]);
                    int month = Byte.Parse(buttonContent[0].Split('.')[1]);
                    if (date.Month == month && (date.Day - 3 < day && date.Day + 4 > day))
                    {
                        DateScrollViewer.ScrollToVerticalOffset(Math.Abs((childNum - 6)) * (sChild as Button).Height);
                        (sChild as Button).Background = Brushes.DarkSlateBlue;
                        //Default is sunday. so substract 1.
                        var wr = new WeekRange(date.AddDays(-((int)date.DayOfWeek) + 1), date, month, new WeekRange().WeekNo);
                        DateWeekActualButtonOnClick(sChild as Button, new RoutedEventArgs(MouseLeftButtonDownEvent), wr);
                        //GenerateSchedule(wr, sChild as Button);
                        break;
                    }
                }
                childNum++;
            }
        }
        #endregion // DateGenerationLogic  
        #endregion // ScheduleGenerationLogic
        #region FilterSelection
        private void FilterSelectionSlideBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch ((int)e.NewValue)
            {
                case 0:
                    FilterSelectionLabel.Content = "Common Schedule";
                    _selectionFilter = SelectionFilter.Common;
                    break;
                case 1:
                    FilterSelectionLabel.Content = "Common Free-time";
                    _selectionFilter = SelectionFilter.FreeTime;
                    break;
                case 2:
                    FilterSelectionLabel.Content = "None";
                    _selectionFilter = SelectionFilter.None;
                    break;
            }
        }
        #endregion // FilterSelection
        #endregion // Schedule logic

        #region SettingsPanelLogic   
        #region SettingsPanelIsChecked
        private void ConfigRememberUsernameCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ConfigRememberUsernameCheckBox.IsChecked != null && _controller != null)
                _controller.Config.ConfigVariables.IsRememberUserName = ConfigRememberUsernameCheckBox.IsChecked.Value;
        }

        private void ConfigStayLoggedInCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ConfigStayLoggedInCheckBox.IsChecked != null && _controller != null)
                _controller.Config.ConfigVariables.IsStayLoggedIn = ConfigStayLoggedInCheckBox.IsChecked.Value;
        }

        private void ConfigCheckForUpdatesCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ConfigCheckForUpdatesCheckBox.IsChecked != null && _controller != null)
                _controller.Config.ConfigVariables.IsCheckForUpdates = ConfigCheckForUpdatesCheckBox.IsChecked.Value;
        }

        private void ConfigLoadCfgFromXmlCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ConfigLoadCfgFromXmlCheckBox.IsChecked != null && _controller != null)
                _controller.Config.ConfigVariables.IsLoadConfigFromXml = ConfigLoadCfgFromXmlCheckBox.IsChecked.Value;
        }

        private void ConfigUseCacheCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ConfigUseCacheCheckBox.IsChecked != null && _controller != null)
                _controller.Config.ConfigVariables.IsUseCache = ConfigUseCacheCheckBox.IsChecked.Value;
        }

        private void ConfigShowDebugConsoleCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ConfigShowDebugConsoleCheckBox.IsChecked != null && _controller != null)
                _controller.Config.ConfigVariables.IsShowDebugConsole = ConfigShowDebugConsoleCheckBox.IsChecked.Value;
            if (ConfigShowDebugConsoleCheckBox.IsChecked != null && ConfigShowDebugConsoleCheckBox.IsChecked.Value)
            {
                SetConsolePosition((int)this.Left, (int)(this.Top + 20));
            }
            else if (!Environment.GetCommandLineArgs().Contains("-debug") && ConfigShowDebugConsoleCheckBox.IsChecked != null && !ConfigShowDebugConsoleCheckBox.IsChecked.Value)
            {
                ShowWindow(GetConsoleWindow(), 0);
            }
        }
        #endregion // SettingsPanelIsChecked

        #region SettingsPanelInit
        private void ConfigRememberUsernameCheckBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ConfigRememberUsernameCheckBox.Unchecked += ConfigRememberUsernameCheckBox_OnChecked;
            ConfigRememberUsernameCheckBox.IsChecked = _controller.Config.ConfigVariables.IsRememberUserName;
        }

        private void ConfigStayLoggedInCheckBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ConfigStayLoggedInCheckBox.Unchecked += ConfigStayLoggedInCheckBox_OnChecked;
            ConfigStayLoggedInCheckBox.IsChecked = _controller.Config.ConfigVariables.IsStayLoggedIn;
        }

        private void ConfigCheckForUpdatesCheckBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ConfigCheckForUpdatesCheckBox.Unchecked += ConfigCheckForUpdatesCheckBox_OnChecked;
            ConfigCheckForUpdatesCheckBox.IsChecked = _controller.Config.ConfigVariables.IsCheckForUpdates;
        }

        private void ConfigLoadCfgFromXmlCheckBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ConfigLoadCfgFromXmlCheckBox.Unchecked += ConfigLoadCfgFromXmlCheckBox_OnChecked;
            ConfigLoadCfgFromXmlCheckBox.IsChecked = _controller.Config.ConfigVariables.IsLoadConfigFromXml;
        }

        private void ConfigUseCacheCheckBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ConfigUseCacheCheckBox.Unchecked += ConfigUseCacheCheckBox_OnChecked;
            ConfigUseCacheCheckBox.IsChecked = _controller.Config.ConfigVariables.IsUseCache;
        }

        private void ConfigShowDebugConsoleCheckBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ConfigShowDebugConsoleCheckBox.Unchecked += ConfigShowDebugConsoleCheckBox_OnChecked;
            ConfigShowDebugConsoleCheckBox.IsChecked = _controller.Config.ConfigVariables.IsShowDebugConsole;
        }
        #endregion // SettingsPanelInit
        #endregion // SettingPanelLogic

        #region SharedCommonLogic
        /// <summary>
        /// Clones UIElement via xaml reader/writer.
        /// </summary>
        /// <param name="uiElement"></param>
        /// <returns></returns>
        private UIElement CloneUIElement(UIElement uiElement)
        {
            var xamlElement = XamlWriter.Save(uiElement);
            var xamlString = new StringReader(xamlElement);
            var xmlTextReader = new XmlTextReader(xamlString);
            return (UIElement)XamlReader.Load(xmlTextReader);
        }

        private void CloseAllDialogs()
        {
            CardFilterFoundStudentsCard.Visibility = Visibility.Collapsed;
            ShadowDimmGridGrid.Visibility = Visibility.Collapsed;
            FilterCard.Visibility = Visibility.Collapsed;
            CardFoundStudent.Visibility = Visibility.Collapsed;
            ExitSelectionGrid.Visibility = Visibility.Collapsed;
        }

        private bool IsDialogShown()
        {
            if (CardFoundStudent.Visibility == Visibility.Visible ||
                FilterCard.Visibility == Visibility.Visible ||
                CardFilterFoundStudentsCard.Visibility == Visibility.Visible ||
                ExitSelectionGrid.Visibility == Visibility.Visible)
                return true;
            return false;
        }

        private void SetUIElementContentWithStudent(UIElement uiElement, Student student)
        {
            if (uiElement as Button != null)
                (uiElement as Button).Content =
                    student.Name + "," +
                    student.Surname + "\n ," +
                    student.OsId;
            (uiElement as Button).Padding = new Thickness(0);
        }
        #endregion SharedCommonLogic

        #region DLLIMPORT
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        /// <summary>
        /// Searched MC documentation, and felt urge to comment this -_-.
        /// </summary>
        /// <param name="hWnd"> Console window handle</param>
        /// <param name="opt_hWnd">Enum for win position, -1 for topmost, 0 for top, and some more</param>
        /// <param name="x">x from left</param>
        /// <param name="y">y from top</param>
        /// <param name="cx">x from left + width</param>
        /// <param name="cy">y from top + height</param>
        /// <param name="uFlags">No idea</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr opt_hWnd, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void SetConsolePosition(int left, int top)
        {
            //left = left + (int)this.Width + 200;
            ShowWindow(GetConsoleWindow(), 1);
            SetWindowPos(GetConsoleWindow(), IntPtr.Zero, left, top + (int)this.ActualHeight, (int)(this.ActualWidth * 1.3), (int)this.ActualHeight, 0);
        }
        #endregion


    }
}
