using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.AllServices;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.ScheduleManager
{
    internal class FillerScheduleActionManager
    {
        private readonly BaseDebug _debug;
        private readonly AllServices _allServices;
        private readonly Dictionary<List<Student>, List<ScheduleAction>> _commonFillablesDictionaryScheduleActionsByStudents = new Dictionary<List<Student>, List<ScheduleAction>>();
        private Thread _workerThread;
        protected List<Task<List<Dictionary<List<Student>, List<ScheduleAction>>>>> Works = new List<Task<List<Dictionary<List<Student>, List<ScheduleAction>>>>>();

        public FillerScheduleActionManager(BaseDebug debug, AllServices allServices)
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

        #region FillableScheduleFunctionality
        /// <summary>
        /// Iterates trough schedules of all provided students, and by prioritising the first one, 
        /// common ActionSchedules with all provided students are found and returned. 
        /// </summary>
        /// <param name="students"></param>
        /// <param name="fromDateTime"></param>
        /// <returns></returns>
        public Dictionary<List<Student>, List<ScheduleAction>> GetUncommonScheduleActionsByStudents(List<Student> students, DateTime fromDateTime = new DateTime(), string semester = "")
        {
            Task<List<Dictionary<List<Student>, List<ScheduleAction>>>> task = new Task<List<Dictionary<List<Student>, List<ScheduleAction>>>>(() =>
            {
                List<ScheduleAction> fillableActions = new List<ScheduleAction>();
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
                                // If idno is different, there will be delta in time, so compare that and create 
                                // list of possible empty schedule actions, then, merge them into big one scheduleAction.
                                if (scheduleAction.ScheduleActionIdno != priorityScheduleAction.ScheduleActionIdno)
                                {
                                    if (
                                        priorityScheduleAction.DateAbsoluteFrom != null &&
                                       priorityScheduleAction.DateAbsoluteFrom.Value.GetDateTime() > fromDateTime &&
                                        scheduleAction.DateAbsoluteFrom != null &&
                                        scheduleAction.DateAbsoluteFrom.Value.GetDateTime() > fromDateTime)
                                    {
                                        //Když nezačne dřív, než já skončím, máme spol. volný čas.
                                        if (scheduleAction.Semester == priorityScheduleAction.Semester &&
                                            scheduleAction.Day == priorityScheduleAction.Day &&
                                            ((scheduleAction.HourAbsoluteTo.Value.GetDateTime() <
                                              priorityScheduleAction.HourAbsoluteFrom.Value.GetDateTime() &&
                                              ((priorityScheduleAction.HourAbsoluteFrom.Value.GetDateTime() -
                                                scheduleAction.HourAbsoluteTo.Value.GetDateTime()).Minutes < 40) &&
                                              ((priorityScheduleAction.HourAbsoluteTo.Value.GetDateTime() -
                                                scheduleAction.HourAbsoluteFrom.Value.GetDateTime()).Hours > 2)
                                              ))
                                            )
                                        {
                                            // Create new ScheduleAction based on priority scheduleAction. 
                                            // Then modifies it to create delta between prio date and slave schedule date.
                                            ScheduleAction fillableScheduleAction = new ScheduleAction()
                                            {
                                                Name = "[" + scheduleAction.Name + "]_To_[" + priorityScheduleAction.Name + "]",
                                                ScheduleActionIdno = scheduleAction.ScheduleActionIdno + "_free",
                                                HourAbsoluteFrom = scheduleAction.HourAbsoluteTo.Value,
                                                HourAbsoluteTo = priorityScheduleAction.HourAbsoluteFrom.Value,
                                                Day = priorityScheduleAction.Day,
                                                Subject = "Break",
                                                LectureType = "SpolVolno",
                                                Semester = priorityScheduleAction.Semester,

                                            };
                                            // Set priority time (end of my lecture), then, look for start of other person lecture. delta will be our free time.
                                            //fillableScheduleAction.HourAbsoluteFrom.Value.SetDateTime(priorityScheduleAction.HourAbsoluteFrom.Value.GetDateTime());
                                            //fillableScheduleAction.HourAbsoluteTo.Value.SetDateTime(scheduleAction.HourAbsoluteFrom.Value.GetDateTime());
                                            // If scheduleAction is not present in list, add it there. Take idno from preffered.
                                            if (fillableActions.Count == 0 || fillableActions.Exists(action =>
                                                    action.ScheduleActionIdno != priorityScheduleAction.ScheduleActionIdno))
                                                fillableActions.Add(fillableScheduleAction);
                                        }
                                    }
                                }
                                else
                                {
                                    // If scheduleAction idno is same, there is no "free time".
                                    // Only if scheduleAction is prostponed (odložená) or cancelled, 
                                    // we have a reason to check it. TODO: impl prostpone detection.
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

                List<ScheduleAction> _commonFillableActions = new List<ScheduleAction>();
                foreach (var scheduleAction in fillableActions)
                {
                    if (!_commonFillableActions.Contains(scheduleAction))
                    {
                        _commonFillableActions.Insert(0,scheduleAction);
                    }
                }

                fillableActions = _commonFillableActions;
                // For free time, there has to be absolutely no common schedules. 
                if (!FindAndFilterCommonActionsByStudents(students, fillableActions, out var commonScheduleActionsFiltered))
                {
                    if (!_commonFillablesDictionaryScheduleActionsByStudents.ContainsKey(students))
                        _commonFillablesDictionaryScheduleActionsByStudents.Add(students, fillableActions);
                    else
                    {
                        _commonFillablesDictionaryScheduleActionsByStudents.Remove(students);
                        _commonFillablesDictionaryScheduleActionsByStudents.Add(students, fillableActions);
                    }
                }
                else
                {
                    throw new ManagerException("Specified students have no possible free common schedule actions!");
                }
                return new List<Dictionary<List<Student>, List<ScheduleAction>>>() { _commonFillablesDictionaryScheduleActionsByStudents };
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
                    // (If there is no common action even in one student, there is no reason to 
                    // check it for everybody..)
                    if (!_allServices.ScheduleService.GetScheduleByStudent(student)
                        .ScheduleJson.ScheduleActions.Contains(common))
                    {
                        contains = false;
                        break;
                    }
                    else
                    {
                        // Set temp. val for future use. (right below :-D)
                        // If every student passes, common action is viable.
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
                _commonFillablesDictionaryScheduleActionsByStudents.Add(students, commonActions2);
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
            if (_commonFillablesDictionaryScheduleActionsByStudents.TryGetValue(students, out var scheduleActions))
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
                    _debug.AddMessage_Assync<object>(new Message<object>("Common subject filtered by specified teacher: " + teacher.ToString(), MessageTypeEnum.Indifferent)).Wait(-1);
                    string studentsStr = "";
                    foreach (var student in students)
                    {
                        studentsStr += student.ToString();
                    }
                    _debug.AddMessage_Assync<object>(new Message<object>("and students: " + studentsStr, MessageTypeEnum.Indifferent)).Wait(-1);
                    return scheduleActionsTemp;
                }
            }
            _debug.AddMessage_Assync<object>(new Message<object>("No common subject of specified students with specified teacher: " + teacher.ToString(), MessageTypeEnum.Error)).Wait(-1);
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

            if (_commonFillablesDictionaryScheduleActionsByStudents.TryGetValue(students, out var scheduleActions))
            {
                foreach (var scheduleAction in scheduleActions)
                {
                    _allServices.ScheduleService.WriteScheduleActionInfo(scheduleAction);
                    Thread.Sleep(1);
                }
                _debug.AddMessage_Assync<object>(new Message<object>(nameof(ScheduleAction) + " count:" + scheduleActions.Count)).Wait();
            }
            else _debug.AddMessage_Assync<object>(new Message<object>("Cannot write empty schedule action", MessageTypeEnum.Error)).Wait();
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

