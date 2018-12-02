using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.AllServices
{
    internal class AllServices
    {
        public readonly StudentService StudentService;
        public readonly TeacherService TeacherService;
        public readonly ScheduleService ScheduleService;

        public AllServices(StudentService studentService, TeacherService teacherService, ScheduleService scheduleService)
        {
            this.StudentService = studentService;
            this.TeacherService = teacherService;
            this.ScheduleService = scheduleService;
        }

    }
}
