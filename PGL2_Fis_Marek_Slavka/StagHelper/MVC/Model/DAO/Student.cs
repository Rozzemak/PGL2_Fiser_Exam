using System;
using Newtonsoft.Json;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO
{
    /// <summary>
    ///     This is model for json send by Stag Rest API.
    /// </summary>
    internal struct Student
    {
        /// <summary>
        ///     I had to disable this annoing warning, since this is just a struct.
        ///     Also, dont fear the bracket syntax, i hate when i have to go by reflexive naming convention,
        ///     cause stag output is in cs language, coding should be in english && the name of properties too.
        /// </summary>
#pragma warning disable 0649
        [JsonProperty("osCislo")] public string OsId;
        [JsonProperty("jmeno")] public string Name;
        [JsonProperty("prijmeni")] public string Surname;
        [JsonProperty("titulPred")] public string TitleBeforeName;
        [JsonProperty("titulZa")] public string TitleAfterName;
        [JsonProperty("stav")] public string State;
        [JsonProperty("userName")] public string UserName;
        [JsonProperty("stprIdno")] public uint StprIdno;
        [JsonProperty("nazevSp")] public string NameOfCourse;
        [JsonProperty("fakultaSp")] public string FacultySid;
        [JsonProperty("kodSp")] public string CodeSp;
        [JsonProperty("formaSp")] public string StudiumForm;
        [JsonProperty("typSp")] public string TypeOfStudium;
        [JsonProperty("czvSp")] public string CzvSp;
        [JsonProperty("mistoVyuky")] public string PlaceOfTeaching;
        [JsonProperty("rocnik")] public uint GradeActual;
        [JsonProperty("financovani")] public string Finances;
        [JsonProperty("oborKomb")] public string SpecComb;
        [JsonProperty("oborIdnos")] public string SpecIDnos;
        [JsonProperty("email")] public string Email;
        [JsonProperty("maxDobaDatum")] private string _maxDateStudyLenght;

        public DateTime MaxDateStudyLenght
        {
            get
            {
                if (DateTime.TryParse(_maxDateStudyLenght, out var tempDate))
                    return tempDate;
                return new DateTime();
            }
        }

        [JsonProperty("simsP58")] public string SimsP58;
        [JsonProperty("simsP59")] public string SimsP59;
        [JsonProperty("cisloKarty")] public string NoOfCard;
        [JsonProperty("pohlavi")] public string Gender;
        [JsonProperty("rozvrhovyKrouzek")] public string ScheduleHobby;
        [JsonProperty("studijniKruh")] public string StudyGroup;
        [JsonProperty("evidovanBankovniUcet")] public string BankAccountEvidated;
#pragma warning restore 0649

        public override string ToString()
        {
            return "{[" + OsId + "]" + "[" + Name + "]" + "[" + Surname + "]}";
        }
    }
}