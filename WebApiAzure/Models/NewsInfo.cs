using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class NewsInfo
    {
        #region Private Members
        long id;
        string title;
        string details;
        DateTime date;
        bool isWeeklyFocus;
        bool isMonthlyFocus;
        #endregion

        #region Constructors
        public NewsInfo()
        {
            id = 0;
            title = "";
            details = "";
            date = DateTime.Today;
            isWeeklyFocus = false;
            isMonthlyFocus = false;
        }
        public NewsInfo(long id, string title, DateTime date, bool isWeeklyFocus, bool isMonthlyFocus, int xIconID)
        {
            this.id = id;
            this.title = title;
            this.date = date;
            this.isWeeklyFocus = isWeeklyFocus;
            this.isMonthlyFocus = isMonthlyFocus;
        }
        #endregion

        #region Public Methods
        public int GetDaysDue()
        {
            TimeSpan ts = date.Subtract(DateTime.Today);
            return ts.Days;
        }
        #endregion

        #region Public Properties
        public long ID
        {
            get { return id; }
            set { id = value; }
        }
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        public string Details
        {
            get { return details; }
            set { details = value; }
        }
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }
        public bool IsWeeklyFocus
        {
            get { return isWeeklyFocus; }
            set { isWeeklyFocus = value; }
        }
        public bool IsMonthlyFocus
        {
            get { return isMonthlyFocus; }
            set { isMonthlyFocus = value; }
        }
        #endregion
    }
}