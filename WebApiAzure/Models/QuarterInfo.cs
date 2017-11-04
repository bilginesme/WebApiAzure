using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class QuarterInfo
    {
        public enum QuarterEnum : int { First = 1, Second = 2, Third = 3, Fourth = 4 }

        #region Private Members
        int quarterID;
        QuarterEnum quarter;
        DateTime startDate, endDate;
        Dictionary<int, MonthInfo> months;
        Dictionary<int, float> totals;
        string label, theme;
        int performance;
        #endregion

        #region Constructors
        public QuarterInfo()
        {
            quarterID = 0;
            quarter = QuarterEnum.First;
            startDate = DateTime.Today;
            endDate = DateTime.Today;
            months = new Dictionary<int, MonthInfo>();
            totals = new Dictionary<int, float>();
            label = "";
            theme = "";
            performance = 0;
        }
        public QuarterInfo(DateTime theDate)
        {
            quarterID = 0;
            quarter = (QuarterEnum)(int)((theDate.Month - 1) / 3 + 1);
            startDate = new DateTime(theDate.Year, 1, 1).AddMonths(3 * (((int)quarter) - 1));
            endDate = new MonthInfo(startDate.AddMonths(2)).EndDate;
            months = new Dictionary<int, MonthInfo>();
            totals = new Dictionary<int, float>();
            label = "";
            theme = "";
            performance = 0;
        }
        public QuarterInfo(QuarterEnum quarter, int year)
        {
            quarterID = 0;
            this.quarter = quarter;
            startDate = new DateTime(year, 1, 1).AddMonths(3 * (((int)quarter) - 1));
            endDate = new MonthInfo(startDate.AddMonths(2)).EndDate;
            months = new Dictionary<int, MonthInfo>();
            totals = new Dictionary<int, float>();
            label = "";
            theme = "";
            performance = 0;
        }
        #endregion

        #region Public Methods
        public bool CheckIsThisThePresentQuarter()
        {
            if (DateTime.Now > StartDate && DateTime.Now < EndDate) return true;
            else return false;
        }
        public string GetDescription()
        {
            return System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(startDate.Month)
                + " /" + System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(endDate.Month);
        }
        #endregion

        #region Public Properties
        public int QuarterID { get { return quarterID; } set { quarterID = value; } }
        public QuarterEnum Quarter { get { return quarter; } }
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime EndDate { get { return endDate; }}
        public int Year { get { return startDate.Year; }}
        public Dictionary<int, MonthInfo> Months { get { return months; } set { months = value; }}
        public Dictionary<int, float> Totals { get { return totals; } set { totals = value; }}
        public string Label { get { return label; } set { label = value; }}
        public string Theme { get { return theme; } set { theme = value; }}
        public int Performance { get { return performance; } set { performance = value; }}
        #endregion
    }
}