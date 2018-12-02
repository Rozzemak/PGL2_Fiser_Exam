using Newtonsoft.Json;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO
{
    internal struct Teacher
    {
#pragma warning disable 0649
        [JsonProperty("ucitIdno")] public string TeacherIdno;
        [JsonProperty("jmeno")] public string Name;
        [JsonProperty("prijmeni")] public string Surname;
        [JsonProperty("titulPred")] public string TitleBeforeName;
        [JsonProperty("titulZa")] public string TitleAfterName;
        [JsonProperty("platnost")] public string Valid;
        [JsonProperty("zamestnanec")] public string Employee;
        [JsonProperty("podilNaVyuce")] public string LectureAttendance;
#pragma warning restore 0649

        public override string ToString()
        {
            return "["+TeacherIdno +"]["+ Name +"]["+ Surname+"]";
        }
    }
}