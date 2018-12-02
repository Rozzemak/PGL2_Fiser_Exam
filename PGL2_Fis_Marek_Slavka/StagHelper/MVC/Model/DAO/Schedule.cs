using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO
{
    class ScheduleJson
    {
        [JsonProperty("rozvrhovaAkce")]
        public List<ScheduleAction> ScheduleActions { get; set; }
    }

    class Schedule
    {
        public readonly Student Student;
        public ScheduleJson ScheduleJson;

        public Schedule(Student student, ScheduleJson scheduleJson)
        {
            this.Student = student;
            this.ScheduleJson = scheduleJson;
        }

        public List<ScheduleAction> GetFreeTimeAsScheduleActions()
        {
            List<ScheduleAction> scheduleActions = new List<ScheduleAction>();

            foreach (var scheduleAction in this.ScheduleJson.ScheduleActions)
            {
                //scheduleActions = this.ScheduleJson.ScheduleActions.FindAll(action => action.HourAbsoluteTo.Value.GetDateTime() != default(DateTime) &&
                //                                                                      action.Semester == scheduleAction.Semester &&
                //                                                                      action.Semester != "ZL" &&
                //                                                                      scheduleAction.HourAbsoluteFrom.Value.GetDateTime() < action.HourAbsoluteFrom.Value.GetDateTime());
                foreach (var scheduleAction2 in ScheduleJson.ScheduleActions)
                {
                    if (
                        scheduleAction2.Semester == scheduleAction.Semester &&
                        scheduleAction2.Semester != "ZL" &&
                        scheduleAction.HourAbsoluteFrom.Value.GetDateTime() > scheduleAction2.HourAbsoluteFrom.Value.GetDateTime())
                    {
                        ScheduleAction fillableScheduleAction = new ScheduleAction()
                        {
                            Name = "[" + scheduleAction.Subject + "]_To_[" + scheduleAction2.Subject + "]",
                            ScheduleActionIdno = scheduleAction.ScheduleActionIdno + "_free",
                            HourAbsoluteFrom = scheduleAction.HourAbsoluteTo.Value,
                            HourAbsoluteTo = scheduleAction.HourAbsoluteFrom.Value,
                            Day = scheduleAction.Day,
                            Subject = "Break"
                        };
                        if (scheduleActions.Count == 0 || scheduleActions.Exists(action => action.ScheduleActionIdno != fillableScheduleAction.ScheduleActionIdno))
                            scheduleActions.Add(fillableScheduleAction);
                    }
                }
            }
            return scheduleActions;
        }

    }
}
