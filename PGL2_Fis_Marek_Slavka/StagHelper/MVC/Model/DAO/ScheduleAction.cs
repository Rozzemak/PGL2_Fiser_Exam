using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO
{
    struct DateJson
    {
        [JsonProperty("value")] public string Value;

        public DateTime GetDateTime()
        {
            return DateTime.Parse(Value);
        }

        public void SetDateTime(DateTime dateTime)
        {
            this.Value = dateTime.ToString();
        }

        public override string ToString()
        {
            return Value;
        }
    }
 
    internal struct ScheduleAction
    {
#pragma warning disable 0649
        [JsonProperty("roakIdno")] public string ScheduleActionIdno;
        [JsonProperty("nazev")] public string Name;
        [JsonProperty("katedra")] public string Department;
        [JsonProperty("predmet")] public string Subject;
        [JsonProperty("statut")] public string Status;
        [JsonProperty("ucitIdno")] public string TeacherIdno;
        [JsonProperty("ucitel")] public Teacher? Teacher;
        [JsonProperty("rok")] public string Year;
        [JsonProperty("budova")] public string Building;
        [JsonProperty("mistnost")] public string Room;
        [JsonProperty("kapacitaMistnosti")] public string RoomCapacity;
        [JsonProperty("planObsazeni")] public string FillPlan;
        [JsonProperty("obsazeni")] public string FillActual;
        [JsonProperty("typAkce")] public string LectureType;
        [JsonProperty("typAkceZkr")] public string LectureTypeAbrv;
        [JsonProperty("semestr")] public string Semester;
        [JsonProperty("platnost")] public string Validity;
        [JsonProperty("den")] public string Day;
        [JsonProperty("denZkr")] public string DayAbrv;
        [JsonProperty("hodinaOd")] public string HourRelativeFrom;
        [JsonProperty("hodinaDo")] public string HourRelativeTo;
        [JsonProperty("hodinaSkutOd")] public DateJson? HourAbsoluteFrom;
        [JsonProperty("hodinaSkutDo")] public DateJson? HourAbsoluteTo;
        [JsonProperty("tydenOd")] public string WeekFrom;
        [JsonProperty("tydenDo")] public string WeekTo;
        [JsonProperty("tyden")] public string WeekRepetetivness;
        [JsonProperty("tydenZkr")] public string WeekRepetetivnessAbrv;
        [JsonProperty("grupIdno")] public string GrupIdno;
        [JsonProperty("jeNadrazena")] public string IsSuperior;
        [JsonProperty("maNadrazenou")] public string HasSuperior;
        [JsonProperty("kontakt")] public string Contact;
        [JsonProperty("krouzky")] public string Clubs;
        [JsonProperty("casovaRada")] public string Timeline;
        [JsonProperty("datum")] public DateJson? Date;
        [JsonProperty("datumOd")] public DateJson? DateAbsoluteFrom;
        [JsonProperty("datumDo")] public DateJson? DateAbsoluteTo;
        [JsonProperty("druhAkce")] public string TypeOfAction;
        [JsonProperty("vsichniUciteleUcitIdno")] public string AllTeacherIdno;
        [JsonProperty("vsichniUciteleJmenaTituly")] public string AllTeacherNameTitles;
        [JsonProperty("vsichniUciteleJmenaTitulySPodily")] public string AllTeacherNameTitlesWithAttendance;
        [JsonProperty("vsichniUcitelePrijmeni")] public string AllTeacherSurname;
        [JsonProperty("referencedIdno")] public string ReferenceIdno;
        [JsonProperty("poznamkaRozvrhare")] public string NoteForDispatcher;
        [JsonProperty("nekonaSe")] public string IsNotScheduled;
        [JsonProperty("owner")] public string Owner;
        [JsonProperty("zakazaneAkce")] public string ForbiddenActions;
        [JsonProperty("vyucJazyk")] public string LectureLanguage;
#pragma warning restore 0649

        public double GetScheduleActionHourRelativeLength()
        {
            return (this.HourAbsoluteTo.Value.GetDateTime() - HourAbsoluteFrom.Value.GetDateTime()).TotalHours;
        }

        public double GetRelativeSchoolHourFromAbsolute()
        {
            return this.HourAbsoluteFrom.Value.GetDateTime().Hour - 7;
        }
    }
}
