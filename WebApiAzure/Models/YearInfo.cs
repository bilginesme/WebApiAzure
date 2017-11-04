using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class YearInfo
    {
        #region Private Members
        int yearID;
        DateTime startDate, endDate;
        string label, theme;
        int year;
        List<MonthInfo> months = new List<MonthInfo>();
        int performance;
        #endregion

        #region Constructors
        public YearInfo()
        {
            yearID = 0;
            year = DateTime.Today.Year;

            label = "";
            theme = "";

            startDate = new DateTime(year, 1, 1);
            endDate = startDate.AddYears(1).AddDays(-1);

            for (int i = 1; i <= 12; i++)
                months.Add(new MonthInfo(new DateTime(year, i, 1)));

            performance = 0;
        }
        public YearInfo(int year)
        {
            this.year = year;
            yearID = 0;

            label = "";
            theme = "";

            startDate = new DateTime(year, 1, 1);
            endDate = startDate.AddYears(1).AddDays(-1);

            for (int i = 1; i <= 12; i++)
                months.Add(new MonthInfo(new DateTime(year, i, 1)));

            performance = 0;
        }
        #endregion

        #region Public Methods
        public int GetNumberOfDays()
        {
            int result = 0;
            for (int i = 1; i <= 12; i++) result += DateTime.DaysInMonth(year, i);
            return result;
        }
        public bool CheckIsThisThePresentYear()
        {
            if (year == DateTime.Today.Year) return true;
            else return false;
        }
        #endregion

        #region Public Properties
        public int YearID { get { return yearID; } set { yearID = value; }}
        public string Theme { get { return theme; } set { theme = value; }}
        public string Label { get { return label; } set { label = value; }}
        /// <summary>
        /// First day of the year
        /// </summary>
        public DateTime StartDate { get { return startDate; }}
        /// <summary>
        /// Last day of the year
        /// </summary>
        public DateTime EndDate { get { return endDate; }}
        public int Year { get { return year; }}
        public List<MonthInfo> Months { get { return months; } set { months = value; }}
        /// <summary>
        /// Just gives the number of months. That is 12 :)
        /// </summary>
        public int NumberOfMonths { get { return 12; }}
        public int Performance { get { return performance; } set { performance = value; }}
        #endregion
    }
}