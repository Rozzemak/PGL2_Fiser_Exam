using Newtonsoft.Json;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.Base_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services
{
    class ScheduleService : BaseService<Schedule>
    {
        public ScheduleService(BaseDebug baseDebug, Client client)
            : base(baseDebug, client)
        {

        }


        public Schedule LoadScheduleByStudent(Student student)
        {
            Task<List<Schedule>> task = new Task<List<Schedule>>(() =>
            {
                if (!student.Equals(default(Student)))
                {
                    var request = "rozvrhy/getRozvrhByStudent?" +
                                  "stagUser=" + Client.StagUser.StagOsId +
                                  "&semestr=%25" +
                                  "&outputFormat=json" +
                                  "&osCislo=" + student.OsId +
                                  "&rok=" + (DateTime.Today.Year - 1); // specifikovat tady rok... gradeActual asi,... nepamatuju :-D
                    var result = Client.SendRequest(request);
                    if (result != "" && result != "[null]")
                    {
                        // Since rest for schedules is one of few WS from stag, that needs some form of auth (that i´m gonna use)
                        // I will just throw away any form of html response. I don´t need it. (response is HTML? => throw ex.)
                        if (result.Substring(0, 1).Contains("["))
                        {
                            result = result.Substring(1, result.Length - 2);
                            var scheduleJson = JsonConvert.DeserializeObject<ScheduleJson>(result);
                            var schedule = new Schedule(student, scheduleJson);
                            if (!ServiceCollection.Contains(schedule))
                            {
                                ServiceCollection.Add(schedule);
                                return new List<Schedule>() { schedule };
                            }
                        }
                    }
                    throw new ServiceException("No student schedule found with specified studentOsId:[" + student.OsId + "]");
                }
                throw new ServiceException("Student specified is essentially null");
            });
            AddWork(task);
            return ResultHandler(task).First();
        }

        public Schedule GetScheduleByStudent(Student student)
        {
            var scheduleTemp = ServiceCollection.Find(schedule => schedule.Student.OsId == student.OsId);
            if (scheduleTemp == null || scheduleTemp.Student.Equals(default(Student)))
            {
                Debug.AddMessage<object>(new Message<object>("Schedule NOT cached. Loading from db."));
                scheduleTemp = LoadScheduleByStudent(student);
            }
            return scheduleTemp;
        }

        /// <summary>
        /// By usage of debug class, schedule is written down to be human readable. (And all its actions)
        /// </summary>
        /// <param name="schedule"></param>
        public void WriteScheduleInfo(Schedule schedule)
        {
            if (schedule != null)
            {
                var list = new List<Message<object>>();
                list.Add(new Message<object>(("_ _ _ _ _ _Schedule_ _ _ _ _ _"), MessageTypeEnum.Indifferent));
                foreach (FieldInfo val in schedule.GetType().GetFields())
                {
                    list.Add(new Message<object>(val.Name + ": " + val.GetValue(schedule), MessageTypeEnum.Rest));
                }

                list.Add(new Message<object>(("_ _ _ _ _ _ScheduleJson_ _ _ _ _ _"), MessageTypeEnum.Indifferent));
                foreach (var scheduleAction in schedule.ScheduleJson.ScheduleActions)
                {
                    list.Add(
                        new Message<object>(("_ _ _ _ _ RozvrhovaAkceJson _ _ _ _ _"), MessageTypeEnum.Indifferent));
                    foreach (FieldInfo val in scheduleAction.GetType().GetFields())
                    {
                        list.Add(new Message<object>(val.Name + ": " + val.GetValue(scheduleAction),
                            MessageTypeEnum.Rest));
                    }
                }

                list.Add(new Message<object>(("_ _ _ _ _ _Schedule-END_ _ _ _ _ _"), MessageTypeEnum.Indifferent));
                Debug.AddMessages<object>(list);
            }
        }

        /// <summary>
        /// By usage of debug class, schedule action is written down to be human readable.
        /// </summary>
        /// <param name="scheduleAction"></param>
        public void WriteScheduleActionInfo(ScheduleAction scheduleAction)
        {
            List<Message<object>> messages = new List<Message<object>>();
            if (!scheduleAction.Equals(default(ScheduleAction)))
            {
                messages.Add(
                    new Message<object>(("_ _ _ _ _ RozvrhovaAkceJson _ _ _ _ _"), MessageTypeEnum.Indifferent));
                foreach (FieldInfo val in scheduleAction.GetType().GetFields())
                {
                    if (val.GetValue(scheduleAction) != null)
                        messages.Add(new Message<object>(val.Name + ": " + val.GetValue(scheduleAction),
                        MessageTypeEnum.Rest));
                }
                Debug.AddMessages<object>(messages);
            }
        }


        /// <summary>
        /// By usage of debug class, schedule actions are written down to be human readable.
        /// </summary>
        /// <param name="scheaduleActions"></param>
        public void WriteScheduleActionsInfo(List<ScheduleAction> scheaduleActions)
        {
            List<Message<object>> messages = new List<Message<object>>();
            if(scheaduleActions != null)
            foreach (var scheduleAction in scheaduleActions)
            {           
                messages.Add(
                    new Message<object>(("_ _ _ _ _ RozvrhovaAkceJson _ _ _ _ _"), MessageTypeEnum.Indifferent));
                foreach (FieldInfo val in scheduleAction.GetType().GetFields())
                {
                    if (val.GetValue(scheduleAction) != null)
                            messages.Add(new Message<object>(val.Name + ": " + val.GetValue(scheduleAction),
                        MessageTypeEnum.Rest));
                }
                Debug.AddMessages<object>(messages);
            }
        }

        public override void AddIntoCollectionIfNotLoaded(List<Schedule> toBeAdded)
        {
            throw new NotImplementedException();
        }
    }
}
