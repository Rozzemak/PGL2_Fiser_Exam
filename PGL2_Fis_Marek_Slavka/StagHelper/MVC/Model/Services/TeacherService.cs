using Newtonsoft.Json;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.Base_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services
{
    class TeachersJson
    {
        [JsonProperty("ucitel")]
        public List<Teacher> Teachers { get; set; }
    }

    class TeacherService : BaseService<Teacher>
    {
        public TeacherService(BaseDebug baseDebug, Client client)
            : base(baseDebug, client)
        {

        }


        public List<Teacher> LoadTeacherByName(string surname, string name = "")
        {
            Task<List<Teacher>> task = new Task<List<Teacher>>(() =>
            {
                var request = "ucitel/najdiUcitelePodleJmena?" +
                              "prijmeni=" + surname +
                              "&jmeno=" + name +
                              "&outputFormat=json";
                var result = Client.SendRequest(request);
                if (result != "" && result != "[null]")
                {
                    string modResult = result.Substring(1, result.Length - 2);
                    var teachers = JsonConvert.DeserializeObject<TeachersJson>(modResult);
                    int previousTeacherCount = ServiceCollection.Count;
                    int teacherCountTemp = 0;
                    foreach (var teacher in teachers.Teachers)
                    {
                        if (teacher.Name != "" && teacher.Surname != "")
                        {
                            if (!ServiceCollection.Contains(teacher))
                                ServiceCollection.Add(teacher);
                            teacherCountTemp++;
                        }
                    }
                    if(teacherCountTemp != previousTeacherCount)
                    return this.ServiceCollection;
                }
                throw new ServiceException("No teacher found with specified name:[" + surname + " | " + name + "]");
            });
            AddWork(task);
            return ResultHandler(task);
        }


        public void WriteTeacherInfo(Teacher teacher)
        {
            if (!teacher.Equals(default(Teacher)))
            {
                var list = new List<Message<object>>();
                list.Add(new Message<object>(("_ _ _ _ _ _teacherJson_ _ _ _ _ _"), MessageTypeEnum.Indifferent));
                foreach (FieldInfo val in teacher.GetType().GetFields())
                {
                    list.Add(new Message<object>(val.Name + ": " + val.GetValue(teacher), MessageTypeEnum.Rest));
                }

                Debug.AddMessages<object>(list);
            }
        }

        public void WriteAllTeachersInfo()
        {
            var list = new List<Message<object>>();
            foreach (var teacher in ServiceCollection)
            {
                list.Add(new Message<object>(("_ _ _ _ _ _teacherJson_ _ _ _ _ _"), MessageTypeEnum.Indifferent));
                foreach (FieldInfo val in teacher.GetType().GetFields())
                {
                    list.Add(new Message<object>(val.Name + ": " + val.GetValue(teacher), MessageTypeEnum.Rest));
                }
            }
            Debug.AddMessages<object>(list);
        }


        /// <summary>
        /// Gets first teacher from cache by find(surname,name). If one does not exit, loads one from database.
        /// All params must not be null. 
        /// </summary>
        /// <param name="surname"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Teacher GetTeacherBySurnameAndName(string surname, string name)
        {
            var teacherTemp = ServiceCollection.Find(teacher => teacher.Surname == surname && teacher.Name == name);
            if (teacherTemp.Surname == null || teacherTemp.Name == null)
            {
                Debug.AddMessage<object>(new Message<object>("Teacher NOT cached. Loading from db."));
                teacherTemp = LoadTeacherByName(surname, name).Find(teacher => teacher.Surname == surname && teacher.Name == name);
            }
            return teacherTemp;
        }

        public override void AddIntoCollectionIfNotLoaded(List<Teacher> toBeAdded)
        {
            throw new NotImplementedException();
        }
    }
}
