using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.AllServices;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.ScheduleManager
{
    internal class ComonScheduleActionManager
    {
        private readonly BaseDebug _debug;
        private readonly AllServices _allServices;
        private readonly Dictionary<List<Student>, List<ScheduleAction>> _commonScheduleActionsByStudents = new Dictionary<List<Student>, List<ScheduleAction>>();
        private Thread _workerThread;
        protected List<Task<List<Dictionary<List<Student>, List<ScheduleAction>>>>> Works = new List<Task<List<Dictionary<List<Student>, List<ScheduleAction>>>>>();

        public ComonScheduleActionManager(BaseDebug debug, AllServices allServices)
        {
            this._debug = debug;
            this._allServices = allServices;
            DoWork();
        }

        private void DoWork()
        {
            _workerThread = new Thread(() =>    
            {
                while (true)
                {
                    Thread.Sleep(33);
                    for (int i = 0; i < Works.Count; i++)
                    {
                        if (Works[i].Status == TaskStatus.Created)
                            Works[i].Start();
                        else if (Works[i].Status == TaskStatus.Faulted)
                        {
                            var ex = Works[i].Exception;
                            var faulted = Works[i].IsFaulted;
                            if (faulted && ex != null)
                            {
                                if (Works[i] as Task<List<Dictionary<List<Student>, List<ScheduleAction>>>> != null)
                                {
                                    _debug.AddMessage_Assync<object>(new Message<object>(this.GetType().Name + " request faulted." + " |TaskID[" + Works[i].Id + "]"
                                        + " |TaskResult[" + (Works[i] as Task<List<Dictionary<List<Student>, List<ScheduleAction>>>>).Exception.InnerException.Message + "]", MessageTypeEnum.Error)).Wait(-1);
                                }
                                else
                                {
                                    _debug.AddMessage_Assync<object>(new Message<object>(this.GetType().Name + " request faulted." + " |TaskID[" + Works[i].Id + "]"
                                       + " |TaskResult[" + Works[i].Exception.Message + "]", MessageTypeEnum.Error)).Wait(-1);
                                    _debug.AddMessage_Assync<object>(new Message<object>(ex.Data, MessageTypeEnum.Exception)).Wait(-1);
                                }
                                Works.RemoveAt(i);
                            };
                        }
                        if (i < Works.Count && Works[i].Status == TaskStatus.RanToCompletion)
                        {
                            Works.RemoveAt(i);
                        }
                    }
                }
            });
            _workerThread.Start();
        }

        private void AddWork(Task<List<Dictionary<List<Student>, List<ScheduleAction>>>> tasks)
        {
            this.Works.Add(tasks);
        }

        #region CommonScheduleFunctionality
        /// <summary>
        /// Iterates trough schedules of all provided students, and by prioritising the first one, 
        /// common ActionSchedules with all provided students are found and returned. 
        /// </summary>
        /// <param name="students"></param>
        /// <param name="fromDateTime"></param>
        /// <returns></returns>
        public Dictionary<List<Student>, List<ScheduleAction>> GetCommonScheduleActionsByStudents(List<Student> students, DateTime fromDateTime = new DateTime())
        {
            Task<List<Dictionary<List<Student>, List<ScheduleAction>>>> task = new Task<List<Dictionary<List<Student>, List<ScheduleAction>>>>(() =>
            {
                List<ScheduleAction> commonActions = new List<ScheduleAction>();
                Schedule prioritisedSchedule = _allServices.ScheduleService.GetScheduleByStudent(students.First());
                if (prioritisedSchedule is null)
                    throw new ManagerException("Getted schedule is null, check your credentials or network access.");
                foreach (var student in students)
                {
                    if (student.OsId != students.First().OsId)
                    {
                        foreach (var priorityScheduleAction in prioritisedSchedule.ScheduleJson.ScheduleActions)
                        {
                            foreach (var scheduleAction in _allServices.ScheduleService.GetScheduleByStudent(student)
                                .ScheduleJson.ScheduleActions)
                            {
                                if (scheduleAction.ScheduleActionIdno == priorityScheduleAction.ScheduleActionIdno)
                                {
                                    // If all compared variables match, add to dictionary, if one or more does not match, trough
                                    // entire iteration, remove it. This ensures, that this filter will apply for all provided students.
                                    if (priorityScheduleAction.DateAbsoluteFrom != null &&
                                        priorityScheduleAction.DateAbsoluteFrom.Value.GetDateTime() > fromDateTime ||
                                        fromDateTime == new DateTime())
                                    {
                                        // Garbage
                                        // I had to make sure, that comparing just ids is sufficient.
                                        /*(((scheduleAction.HourAbsoluteFrom.ToString() ==
                                           priorityScheduleAction.HourAbsoluteFrom.ToString()
                                           && scheduleAction.HourAbsoluteTo.ToString() ==
                                           priorityScheduleAction.HourAbsoluteTo.ToString() &&
                                           priorityScheduleAction.HourAbsoluteTo.ToString() != "")))
                                            || // Eighter relative or absolute time is set in scheduleAction, or both, but that is not relevant.
                                            (((scheduleAction.HourRelativeFrom == priorityScheduleAction.HourRelativeFrom
                                               && scheduleAction.HourRelativeTo == priorityScheduleAction.HourRelativeTo &&
                                               priorityScheduleAction.HourRelativeTo != ""))) =>paste below */
                                        if (true)
                                        {
                                            if (!commonActions.Contains(priorityScheduleAction))
                                                commonActions.Add(priorityScheduleAction);
                                        }
                                    }
                                }
                                else
                                {
                                    // NOT WORKING AS INTENTED! I am stupid.

                                    // If there is action already in list, but one specified student has not met the time && room req.
                                    // It will be removed

                                    // if (commonActions.Contains(priorityScheduleAction))
                                    //    commonActions.Remove(priorityScheduleAction);
                                }
                            }
                            // Garbage No2 -> cannot query in loop (server usage) and nevertheless, this "query" is bad.
                            /* 
                            if(_allServices.StudentService.LoadStudentsByScheduleAction(priorityScheduleAction.ScheduleActionIdno)
                                .Find(student1 =>
                                {
                                    if (student1.Equals((student)))
                                    {
                                        return true;
                                    }
                                    return false;
                                }).OsId != null)
                            if (commonActions.Contains(priorityScheduleAction))
                                commonActions.Remove(priorityScheduleAction);
                                */
                        }
                    }

                    // NO! :D
                    //prioritisedSchedule = _allServices.ScheduleService.GetScheduleByStudent(student);
                }

                if (FindAndFilterCommonActionsByStudents(students, commonActions, out var commonScheduleActionsFiltered))
                {
                    if (!_commonScheduleActionsByStudents.ContainsKey(students))
                        _commonScheduleActionsByStudents.Add(students, commonScheduleActionsFiltered);
                    else
                    {
                        _commonScheduleActionsByStudents.Remove(students);
                        _commonScheduleActionsByStudents.Add(students, commonScheduleActionsFiltered);
                    }
                }
                else
                {
                    throw new ManagerException("Specified students have no common schedule actions!");
                }
                return new List<Dictionary<List<Student>, List<ScheduleAction>>>() {_commonScheduleActionsByStudents};
            });
            AddWork(task);
            return ResultHandler(task).First();
        }

        /// <summary>
        /// Iterates trough every students schedule, finds common scheduleActions, returns them as list.
        /// </summary>
        /// <param name="students">Students, that are expected to have common schedule</param>
        /// <param name="commonActions">said common actions (gonna be filtered)</param>
        /// <param name="commonScheduleActionsContains">acttual filtered common actions</param>
        /// <returns>True if there is any commonAction, false otherwise.</returns>
        private bool FindAndFilterCommonActionsByStudents(List<Student> students, List<ScheduleAction> commonActions, out List<ScheduleAction> commonScheduleActionsContains)
        {

            List<ScheduleAction> commonActions2 = new List<ScheduleAction>();
            bool contains = false;
            ScheduleAction temp = default(ScheduleAction);
            // Iterates trough common actions.
            foreach (var common in commonActions)
            {
                // Iterates every student.
                foreach (var student in students)
                {
                    // If students schedule does not contain common scheduleAction, break cycle,
                    // else => read below..
                    if (!_allServices.ScheduleService.GetScheduleByStudent(student)
                        .ScheduleJson.ScheduleActions.Contains(common))
                    {
                        contains = false;
                        break;
                    }
                    else
                    {
                        // Set temp. val for future use. (right below :-D)
                        temp = common;
                        contains = true;
                    }
                }
                // If the student schedule is indeed common, then add it to common dictionary.
                // Set bool to false and continue cycle.
                if (contains)
                {
                    if (!commonActions2.Contains(temp))
                        commonActions2.Add(temp);
                    contains = false;
                }
            }
            // If there is (x>0) sucessfull iterations(whole), than said students have in common that number 
            // of subjects.
            if (commonActions2.Count > 0)
            {
                _commonScheduleActionsByStudents.Add(students, commonActions2);
                commonScheduleActionsContains = commonActions2;
                return true;
            }
            // If there is none, return null.
            commonScheduleActionsContains = null;
            return false;
        }

        /// <summary>
        /// Will try to get commonScheduleActions by students, iterates trough them, filter ones
        /// that does not match specified teacher. 
        /// </summary>
        /// <param name="students"></param>
        /// <param name="teacher"></param>
        /// <returns></returns>
        public List<ScheduleAction> FilterScheduleActionsByByTeacher(List<Student> students, Teacher teacher)
        {
            if (_commonScheduleActionsByStudents.TryGetValue(students, out var scheduleActions))
            {   
                var scheduleActionsTemp = new List<ScheduleAction>();
                foreach (var scheduleAction in scheduleActions)
                {
                    if (scheduleAction.Teacher != null && scheduleAction.Teacher.Value.TeacherIdno == teacher.TeacherIdno)
                    {
                        scheduleActionsTemp.Add(scheduleAction);
                    }
                }
                if (scheduleActionsTemp.Count != 0)
                {
                    _debug.AddMessage_Assync<object>(new Message<object>("Common subject filtered by specified teacher: " + teacher.ToString(),MessageTypeEnum.Indifferent)).Wait(-1);
                    string studentsStr ="";
                    foreach (var student in students)
                    {
                        studentsStr += student.ToString();
                    }
                    _debug.AddMessage_Assync<object>(new Message<object>("and students: " + studentsStr, MessageTypeEnum.Indifferent)).Wait(-1);
                    return scheduleActionsTemp;
                }
            }
            _debug.AddMessage_Assync<object>(new Message<object>("No common subject of specified students with specified teacher: " + teacher.ToString(),MessageTypeEnum.Error)).Wait(-1);
            return null;
        }

        /// <summary>
        /// By usage of debug class, commonActionSchedules filtered by students are written down to be human readable.
        /// (Gets scheduleActions by usage of dictionary , where key = students.)
        /// </summary>
        /// <param name="students">Students as key for dictionary</param>
        public void WriteAllCommonScheduledActionsByStudents(List<Student> students)
        {
            string msgContent = "";
            foreach (var student in students)
            {
                msgContent += " [" + student.OsId + "] " + "[" + student.Name + "|" + student.Surname + "]; ";
            }
            _debug.AddMessage_Assync<object>(new Message<object>(msgContent, MessageTypeEnum.Indifferent)).Wait(-1);

            if (_commonScheduleActionsByStudents.TryGetValue(students, out var scheduleActions))
            {
                foreach (var scheduleAction in scheduleActions)
                {
                    _allServices.ScheduleService.WriteScheduleActionInfo(scheduleAction);
                }
                _debug.AddMessage_Assync<object>(new Message<object>(nameof(ScheduleAction) + " count:" + scheduleActions.Count)).Wait(-1);
            }
            else _debug.AddMessage_Assync<object>(new Message<object>("Cannot write empty schedule action",MessageTypeEnum.Error)).Wait();
        }
        #endregion

        protected Dictionary<List<Student>, List<ScheduleAction>> ResultHandler(Task<Dictionary<List<Student>, List<ScheduleAction>>> task)
        {
            try
            {
                task.Wait(-1);
                return task.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is ManagerException)
                    {
                        _debug.AddMessage_Assync<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).Wait(-1);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return default(Dictionary<List<Student>, List<ScheduleAction>>);
        }

        protected List<Dictionary<List<Student>, List<ScheduleAction>>> ResultHandler(Task<List<Dictionary<List<Student>, List<ScheduleAction>>>> task)
        {
            try
            {
                task.Wait(-1);
                return task.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is ManagerException)
                    {
                        _debug.AddMessage_Assync<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).Wait(-1);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return new List<Dictionary<List<Student>, List<ScheduleAction>>>() { default(Dictionary<List<Student>, List<ScheduleAction>>) };
        }

    }



}

