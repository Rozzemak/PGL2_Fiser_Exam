using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.WeekRange
{
    public struct WeekRange
    {
        public DateTime Start;
        public DateTime End;
        public int MM;
        public int WeekNo;

        public WeekRange(DateTime _start, DateTime _end, int _mm, int _weekNo)
        {
            Start = _start;
            End = _end;
            MM = _mm;
            WeekNo = _weekNo;
        }
    }

    static class WeekRanger
    {
        
        public static IEnumerable<WeekRange> GetWeekRange(DateTime dtStart, DateTime dtEnd)
        {
            DateTime fWeekStart, dt, fWeekEnd;
            int wkCnt = 1;

            if (dtStart.DayOfWeek != DayOfWeek.Monday)
            {
                fWeekStart = dtStart.AddDays(7 - (int)dtStart.DayOfWeek);
                fWeekEnd = fWeekStart.AddDays(-1);
                IEnumerable<WeekRange> ranges = getMonthRange(new WeekRange(dtStart, fWeekEnd, dtStart.Month, wkCnt++));
                foreach (WeekRange wr in ranges)
                {
                    yield return wr;
                }
                wkCnt = ranges.Last().WeekNo + 1;

            }
            else
            {
                fWeekStart = dtStart;
            }


            for (dt = fWeekStart.AddDays(6); dt <= dtEnd; dt = dt.AddDays(7))
            {


                IEnumerable<WeekRange> ranges = getMonthRange(new WeekRange(fWeekStart, dt, fWeekStart.Month, wkCnt++));
                foreach (WeekRange wr in ranges)
                {
                    yield return wr;
                }
                wkCnt = ranges.Last().WeekNo + 1;
                fWeekStart = dt.AddDays(1);


            }

            if (dt > dtEnd)
            {

                IEnumerable<WeekRange> ranges = getMonthRange(new WeekRange(fWeekStart, dtEnd, dtEnd.Month, wkCnt++));
                foreach (WeekRange wr in ranges)
                {
                    yield return wr;
                }
                wkCnt = ranges.Last().WeekNo + 1;

            }

        }


        public static IEnumerable<WeekRange> getMonthRange(WeekRange weekRange)
        {

            List<WeekRange> ranges = new List<WeekRange>();

            if (weekRange.Start.Month != weekRange.End.Month)
            {
                DateTime lastDayOfMonth = new DateTime(weekRange.Start.Year, weekRange.Start.Month, 1).AddMonths(1).AddDays(-1);
                ranges.Add(new WeekRange(weekRange.Start, lastDayOfMonth, weekRange.Start.Month, weekRange.WeekNo));
                ranges.Add(new WeekRange(lastDayOfMonth.AddDays(1), weekRange.End, weekRange.End.Month, weekRange.WeekNo + 1));

            }
            else
            {
                ranges.Add(weekRange);
            }

            return ranges;

        }

    }

}

