using System;
using System.Collections.Generic;
using System.Text;

namespace WebApiAzure
{
    public class WeekInfo
    {
        #region Members
        public int WeekID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeekNO { get; set; }
        public string Label { get; set; }
        public string Theme { get; set; }
        public int Performance { get; set; }
        #endregion

        #region Constructors
        public WeekInfo()
        {
            WeekID = 0;
            StartDate = GetFirstDayOfWeek(DateTime.Today);
            GenerateData();
            Label = "";
            Theme = "";
            Performance = 0;
        }
        public WeekInfo(DateTime theDate)
        {
            WeekID = 0;
            StartDate = GetFirstDayOfWeek(theDate);
            GenerateData();
            Label = "";
            Theme = "";
            Performance = 0;
        }
        #endregion

        #region Private Methods
        private void GenerateData()
        {
            WeekNO = DTC.Date.GetWeekNumber(StartDate);
            EndDate = GetLastDayOfWeek(StartDate);
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
            if (StartDate <= DateTime.Today && EndDate >= DateTime.Today) return true;
            else return false;
        }
        public MonthInfo GetMajorMonth()
        {
            int first = StartDate.Month;
            int count = 0;
            DateTime date = StartDate;

            while (date < EndDate)
            {
                if (first != date.Month) break;
                date = date.AddDays(1);
                count++;
            }

            if (count > 3)
                return new MonthInfo(StartDate);
            else
                return new MonthInfo(EndDate);
        }
        public string GetYearWeekKey()
        {
            string strWeek = WeekNO.ToString();
            if (strWeek.Length == 1)
                strWeek = "0" + strWeek;
            string key = StartDate.Year.ToString() + strWeek;

            return key;
        }
        #endregion
    }
}
