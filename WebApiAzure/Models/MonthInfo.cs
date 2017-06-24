using System;
using System.Collections.Generic;
using System.Text;

namespace WebApiAzure
{
    public class MonthInfo
    {
        #region Private Members
        int monthID;
        DateTime startDate;
        string label, theme;
        #endregion

        #region Constructors
        public MonthInfo()
        {
            monthID = 0;
            startDate = DateTime.MinValue;
            label = "";
            theme = "";
        }
        public MonthInfo(DateTime theDate)
        {
            startDate = new DateTime(theDate.Year, theDate.Month, 1);
            label = "";
            theme = "";
        } 
        #endregion

        #region Public Methods
        public string GetDayName(int theDay)
        {
            return new DateTime(startDate.Year,startDate.Month, theDay).DayOfWeek.ToString();
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
        public DateTime StartDate { get { return startDate; }}
        public DateTime EndDate { get { return startDate.AddMonths(1).AddDays(-1); }}
        public int Month { get { return startDate.Month; }}
        public int Year { get { return startDate.Year; }}
        public string Name { get { return System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(startDate.Month); }}
        public string ShortName { get { return System.Globalization.DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(startDate.Month); }}
        public string Label { get { return label; } set { label = value; }}
        public string Theme { get { return theme; } set { theme = value; }}
        #endregion
    }
}
