using Newtonsoft.Json;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Enums;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.Base_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PGL2_Fis_Marek_Slavka.Annotations;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services
{
    class StudentsJson
    {
        [JsonProperty("student")]
        public List<Student> Students { get; set; }
    }

    class StudentOfSubjectJson
    {
        [JsonProperty("studentPredmetu")]
        public List<Student> Students { get; set; }
    }


    class StudentService : BaseService<Student>
    {
        public StudentService(BaseDebug debug, Client client)
            : base(debug, client)
        {
            this.Client = client;
            this.Debug = debug;
        }

        public Student LoadStudentByOsId(string osId = "F17026")
        {
            Task<List<Student>> task = new Task<List<Student>>(() =>
            {
                string request =
                    "student/getStudentInfo?" +
                    "stagUser=" + Client.StagUser.StagOsId +
                    "&osCislo=" + osId +
                    "&rok=" + (DateTime.Today.Year - 1)+
                    "&zobrazovatSimsUdaje=true" +
                    "&lang=cs&outputFormat=json";           
                var result = Client.SendRequest(request);
                result.ToString();
                if (result != "" && result != "[null]")
                {
                    result = result.Substring(1, result.Length - 2);
                    var student = JsonConvert.DeserializeObject<Student>(result);
                    if (!ServiceCollection.Contains(student))
                        ServiceCollection.Add(student);
                    return new List<Student>() { student };
                }
                throw new ServiceException("No student found with specified OsId:[" + osId + "]");
            });
            AddWork(task);
            return ResultHandler(task).First();
        }

        public List<Student> LoadStudentsByScheduleAction(string ScheduleActionIdno)
        {
            Task<List<Student>> task = new Task<List<Student>>(() =>
            {
                if (ScheduleActionIdno != null)
                {
                    string request =
                        "student/getStudentiByRoakce?" +
                        "roakIdno=" + ScheduleActionIdno +
                        "&outputFormat=json";
                    var result = Client.SendRequest(request);
                    if (result != "" && result != "[null]" && result != "n")
                    {
                        string modResult = result.Substring(1, result.Length - 2);
                        var students = JsonConvert.DeserializeObject<StudentOfSubjectJson>(modResult);
                        if (students.Students.Count > 0)
                        {
                            AddIntoCollectionIfNotLoaded(students.Students);
                            return students.Students;
                        }
                    }
                }
                else
                {
                    throw new ServiceException("ScheduleAcitonIdno is null:[" + ScheduleActionIdno + "]");
                }
                throw new ServiceException("No student found in specified scheduleRefIdno:[" + ScheduleActionIdno + "]");
            });
            AddWork(task);
            return ResultHandler(task);
        }

        public List<Student> LoadStudentsByNameAndSureName(string name = "%", string surename = "%")
        {
            Task<List<Student>> task = new Task<List<Student>>(() =>
            {
                List<Student> tempStudents = new List<Student>();
                string request =
                    "student/najdiStudentyPodleJmena?stagUser=" + Client.StagUser.StagOsId +
                    "&prijmeni=" + surename +
                    "&jmeno=" + name +
                    "&zobrazovatSimsUdaje=false" +
                    "&outputFormat=json";
                var result = Client.SendRequest(request);
                if (result != "" && result != "[null]" && result != "n")
                {
                    string modResult = result.Substring(1, result.Length - 2);
                    var students = JsonConvert.DeserializeObject<StudentsJson>(modResult);
                    if (students.Students.Count > 0)
                    {
                        AddIntoCollectionIfNotLoaded(students.Students);
                        return students.Students;
                    }
                }
                throw new ServiceException("No student found with specified name&surname:[" + name + "|"+ surename + "]");
            });
            AddWork(task);
            return ResultHandler(task);
        }

        public List<Student> LoadAllStudentsInDepartment(DepartmentsEnum departmentsEnum)
        {
            Task<List<Student>> task = new Task<List<Student>>(() =>
            {
                string result = "";
                string request = "student/getStudentiByFakulta?" +
                                 "stav=S&" +
                                 "fakulta=" + Enum.GetName(typeof(DepartmentsEnum), departmentsEnum) +
                                 "&outputFormat=json&" +
                                 "rok="+ (DateTime.Today.Year - 1);
                result = Client.SendRequest(request);
                if (result != "" && result != "[null]")
                {
                    string modResult = result.Substring(1, result.Length - 2);
                    var students = JsonConvert.DeserializeObject<StudentsJson>(modResult);
                    if (students.Students.Count > 0)
                    {
                        AddIntoCollectionIfNotLoaded(students.Students);
                        return students.Students;
                    } 
                }
                throw new ServiceException("No students found in specified Department!: [" + Enum.GetName(typeof(DepartmentsEnum), departmentsEnum) + "]");
            });
            AddWork(task);
            return ResultHandler(task);
        }

        /// <summary>
        /// Gets first student with specified OsID, NO DUPLO SHOULD EXIST. 
        /// </summary>
        /// <param name="osId"></param>
        /// <returns></returns>
        public Student GetStudentByOsId(string osId)
        {
            var studentTemp = ServiceCollection.Find(student => student.OsId == osId);
            if (studentTemp.OsId == null)
            {
                Debug.AddMessage<object>(new Message<object>("Student NOT cached. Loading from db."));
                studentTemp = LoadStudentByOsId(osId);
            }
            return studentTemp;
        }

        public void WriteStudentInfo(Student student)
        {
            if (!student.Equals(default(Student)))
            {
                List<Message<object>> list = new List<Message<object>>();
                list.Add(new Message<object>(("_ _ _ _ _ _studentJson_ _ _ _ _ _"), MessageTypeEnum.Indifferent));
                foreach (FieldInfo val in student.GetType().GetFields())
                {
                    list.Add(new Message<object>(val.Name + ": " + val.GetValue(student), MessageTypeEnum.Rest));
                }

                Debug.AddMessages<object>(list);
            }
        }

        public void WriteAllStudentInfosCached()
        {
            List<Message<object>> list = new List<Message<object>>();
            foreach (var student in ServiceCollection)
            {
                list.Add(new Message<object>(("_ _ _ _ _ _studentJson_ _ _ _ _ _"), MessageTypeEnum.Indifferent));
                foreach (FieldInfo val in student.GetType().GetFields())
                {
                    list.Add(new Message<object>(val.Name + ": " + val.GetValue(student), MessageTypeEnum.Rest));
                }
            }
            Debug.AddMessages<object>(list);
            Debug.PrintAllPendingMessages();
        }

        override 
        public void AddIntoCollectionIfNotLoaded(List<Student> students)
        {
            foreach (var student in students)
            {
                if (ServiceCollection.Find(student1 => student1.OsId == student.OsId).Equals(default(Student)))
                {
                    ServiceCollection.Add(student);
                }
            }
        }

        public List<Student> GetAllCachedStudents()
        {
            return this.ServiceCollection;
        }
    }

}
