using System;
using System.Collections.Generic;
using System.Text;
using WebApiAzure.Models;

namespace WebApiAzure
{
    public class MonthInfo
    {
        #region Private Members
        int monthID;
        DateTime startDate;
        List<DayInfo> days = new List<DayInfo>();
        Dictionary<int, float> totals = new Dictionary<int, float>();
        string label, theme;
        float avgWeight, minWeight, maxWeight;
        float avgSleep;
        int performance;
        #endregion

        #region Constructors
        public MonthInfo()
        {
            monthID = 0;
            startDate = DateTime.MinValue;
            label = "";
            theme = "";
            avgSleep = 0;
            avgWeight = 0;
            minWeight = 0;
            maxWeight = 0;
            performance = 0;
        }
        public MonthInfo(DateTime theDate)
        {
            startDate = new DateTime(theDate.Year, theDate.Month, 1);
            label = "";
            theme = "";
            for (int i = 1; i <= DateTime.DaysInMonth(startDate.Year, startDate.Month); i++)
                days.Add(new DayInfo(startDate.AddDays(i)));
            avgWeight = 0; avgSleep = 0;
            performance = 0;
            monthID = 0;
        }
        #endregion

        #region Public Methods
        public string GetDayName(int theDay)
        {
            return new DateTime(startDate.Year, startDate.Month, theDay).DayOfWeek.ToString();
        }
        public bool CheckIsThisThePresentMonth()
        {
            if (DateTime.Now > StartDate && DateTime.Now < EndDate) return true;
            else return false;
        }
        #endregion

        #region Public Properties
        public int MonthID { get { return monthID; } set { monthID = value; }}
        public int HowManyDays { get { return DateTime.DaysInMonth(startDate.Year, startDate.Month); ; }}
        /// <summary>
        /// First day of the month
        /// </summary>
        public DateTime StartDate { get { return startDate; }}
        /// <summary>
        /// Last day of the month
        /// </summary>
        public DateTime EndDate { get { return startDate.AddMonths(1).AddDays(-1); }}
        public int Month { get { return startDate.Month; }}
        public int Year { get { return startDate.Year; }}
        public string Name { get { return System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(startDate.Month); }}
        public string ShortName { get { return System.Globalization.DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(startDate.Month); }}
        public List<DayInfo> Days { get { return days; } set { days = value; }}
        public Dictionary<int, float> Totals { get { return totals; } set { totals = value; }}
        public string Label { get { return label; } set { label = value; }}
        public string Theme { get { return theme; } set { theme = value; }}
        public float AvgSleep { get { return avgSleep; } set { avgSleep = value; }}
        public float AvgWeight { get { return avgWeight; } set { avgWeight = value; }}
        public float MinWeight { get { return minWeight; } set { minWeight = value; }}
        public float MaxWeight { get { return maxWeight; } set { maxWeight = value; }}
        public int Performance { get { return performance; } set { performance = value; }}
        #endregion
    }
}
