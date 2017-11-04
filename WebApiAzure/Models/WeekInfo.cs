using System;
using System.Collections.Generic;
using System.Text;

namespace WebApiAzure
{
    public class WeekInfo
    {
        #region Private Members
        DateTime startDate, endDate;
        int weekNO;
        int weekID;
        string label, theme;
        int performance;
        #endregion

        #region Constructors
        public WeekInfo()
        {
            weekID = 0;
            startDate = GetFirstDayOfWeek(DateTime.Today);
            GenerateData();
            label = "";
            theme = "";
            performance = 0;
        }
        public WeekInfo(DateTime theDate)
        {
            weekID = 0;
            startDate = GetFirstDayOfWeek(theDate);
            GenerateData();
            label = "";
            theme = "";
            performance = 0;
        }
        #endregion

        #region Private Methods
        private void GenerateData()
        {
            weekNO = DTC.Date.GetWeekNumber(startDate);
            endDate = GetLastDayOfWeek(startDate);
        }
        /// <summary>
        /// Returns the first day, (i.e.) Monday of the week
        /// </summary>
        /// <param name="theDate">Reference date</param>
        /// <returns>The first day of the week</returns>
        private DateTime GetFirstDayOfWeek(DateTime theDate)
        {
            DateTime result = theDate;
            while (result.DayOfWeek != DayOfWeek.Monday)
                result = result.AddDays(-1);
            return result;
        }
        /// <summary>
        /// Returns the last day, (i.e. Sunday) of the week
        /// </summary>
        /// <param name="theDate">Reference date</param>
        /// <returns>The last day of the week</returns>
        private DateTime GetLastDayOfWeek(DateTime theDate)
        {
            DateTime result = theDate;
            while (result.DayOfWeek != DayOfWeek.Sunday)
                result = result.AddDays(1);
            return result;
        }
        #endregion

        #region Public Methods
        public bool CheckIsThisThePresentWeek()
        {
            if (startDate <= DateTime.Today && endDate >= DateTime.Today) return true;
            else return false;
        }
        public MonthInfo GetMajorMonth()
        {
            int first = startDate.Month;
            int count = 0;
            DateTime date = startDate;

            while (date < endDate)
            {
                if (first != date.Month) break;
                date = date.AddDays(1);
                count++;
            }

            if (count > 3)
                return new MonthInfo(startDate);
            else
                return new MonthInfo(endDate);
        }
        public string GetYearWeekKey()
        {
            string strWeek = weekNO.ToString();
            if (strWeek.Length == 1)
                strWeek = "0" + strWeek;
            string key = startDate.Year.ToString() + strWeek;

            return key;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// First day of the week
        /// </summary>
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                GenerateData();
            }
        }
        /// <summary>
        /// Last day of the week
        /// </summary>
        public DateTime EndDate
        {
            get { return endDate; }
        }
        public int WeekNO
        {
            get { return weekNO; }
        }
        public string Label
        {
            get { return label; }
            set { label = value; }
        }
        public string Theme
        {
            get { return theme; }
            set { theme = value; }
        }
        public int WeekID
        {
            get { return weekID; }
            set { weekID = value; }
        }
        public int Performance
        {
            get { return performance; }
            set { performance = value; }
        }
        #endregion
    }
}
