using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class DayInfo
    {
        #region Private Members
        DateTime startInstance, endInstance;
        bool isProcessed;
        DateTime theDate;
        float weight, sleepAmount;
        string theme, label;
        Dictionary<int, float> totals = new Dictionary<int, float>();
        float numThings, numIdeas, numTodos;
        int dayID;
        string diary;
        int performance;
        int perfWeek, perfMonth, perfQuarter, perfYear, perfDecade, perfLife;
        int numMinutesSpare;
        #endregion

        #region Constructors
        public DayInfo(DateTime thedate)
        {
            this.theDate = thedate;
            startInstance = DateTime.MinValue;
            endInstance = DateTime.MinValue;
            isProcessed = false;
            weight = 0;
            sleepAmount = 0;
            theme = "";
            label = "";
            dayID = 0;
            diary = "";
            performance = 0;
            perfWeek = 0; perfMonth = 0; perfQuarter = 0; perfYear = 0; perfDecade = 0; perfLife = 0;
            numMinutesSpare = 0;
        }
        public DayInfo()
        {
            this.theDate = DateTime.MinValue;
            startInstance = DateTime.MinValue;
            endInstance = DateTime.MinValue;
            isProcessed = false;
            weight = 0;
            sleepAmount = 0;
            theme = "";
            label = "";
            dayID = 0;
            diary = "";
            performance = 0;
            perfWeek = 0; perfMonth = 0; perfQuarter = 0; perfYear = 0; perfDecade = 0; perfLife = 0;
            numMinutesSpare = 0;
        }
        #endregion

        #region Public Methods
        public bool CheckIsThisThePresentDay()
        {
            if (theDate == DateTime.Today) return true;
            else return false;
        }
        public bool CheckIsThisTheLastDayOf(DTC.RangeEnum range)
        {
            bool result = false;

            if (range == DTC.RangeEnum.Week)
            {
                if (theDate.DayOfWeek == DayOfWeek.Sunday) result = true;
            }
            else if (range == DTC.RangeEnum.Month)
            {
                if (theDate.AddDays(1).Day == 1) result = true;
            }
            else if (range == DTC.RangeEnum.Quarter)
            {
                QuarterInfo thisQuarter = new QuarterInfo(theDate);
                QuarterInfo anotherQuarter = new QuarterInfo(theDate.AddDays(1));

                if (thisQuarter.Quarter != anotherQuarter.Quarter) result = true;
            }
            else if (range == DTC.RangeEnum.Year)
            {
                int thisYear = theDate.Year;

                if (theDate.AddDays(1).Year == thisYear + 1) result = true;
            }

            return result;
        }
        public string GetDayCode()
        {
            return DTC.Date.GetCodestring(theDate);
        }
        public string GetSmartDate(bool addYear)
        {
            return DTC.Date.GetSmartDate(theDate, addYear);
        }
        public string GetWeekDay(bool isShort)
        {
            return DTC.Date.GetWeekDay(theDate, isShort);
        }
        public float GetTodaysTotalHours()
        {
            float result = 16;

            TimeSpan ts = endInstance.Subtract(startInstance);

            result = (float)ts.TotalHours;
            if (result <= 0 || result >= 24) result = 16;

            return result;
        }
        public float GetTodaysPassedHours()
        {
            float result = 16;

            TimeSpan ts = DateTime.Now.Subtract(startInstance);
            result = (float)ts.TotalHours;

            if (result <= 0) result = 16;
            if (result > GetTodaysTotalHours()) result = GetTodaysTotalHours();

            return result;
        }
        #endregion

        #region Public Properties
        public int DayID { get { return dayID; } set { dayID = value; }}
        public DateTime TheDate { get { return theDate; } set { theDate = value; }}
        public string Theme { get { return theme; } set { theme = value; }}
        public string Label { get { return label; } set { label = value; }}
        public DateTime StartInstance { get { return startInstance; } set { startInstance = value; }}
        public string StartInstanceHour { get { return DTC.Date.GetHour(startInstance, false); }}
        public string EndInstanceHour { get { return DTC.Date.GetHour(endInstance, false); }}
        public DateTime EndInstance { get { return endInstance; } set { endInstance = value; }}
        public bool IsProcessed { get { return isProcessed; } set { isProcessed = value; }}
        public float Weight { get { return weight; } set { weight = value; }}
        public float SleepAmount { get { return sleepAmount; } set { sleepAmount = value; }}
        public bool HasDiaryEntry { get { return diary.Length > 0;}}
        public Dictionary<int, float> Totals { get { return totals; } set { totals = value; }}
        public float NumIdeas { get { return numIdeas; } set { numIdeas = value; }}
        public float NumThings { get { return numThings; } set { numThings = value; }}
        public float NumTodos { get { return numTodos; } set { numTodos = value; }}
        public string Diary { get { return diary; } set { diary = value; }}
        public int Performance { get { return performance; } set { performance = value; }}
        public int PerfWeek { get { return perfWeek; } set { perfWeek = value; }}
        public int PerfMonth { get { return perfMonth; } set { perfMonth = value; }}
        public int PerfQuarter { get { return perfQuarter; } set { perfQuarter = value; }}
        public int PerfYear { get { return perfYear; } set { perfYear = value; }}
        public int PerfDecade { get { return perfDecade; } set { perfDecade = value; }}
        public int PerfLife { get { return perfLife; } set { perfLife = value; }}
        public int NumMinutesSpare { get { return numMinutesSpare; } set { numMinutesSpare = value; } }
        #endregion
    }
}