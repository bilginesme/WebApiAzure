using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class OwnerInfo
    {
        #region Private Members
        int ownerID;
        DTC.RangeEnum range;
        DateTime startDate, endDate;
        int no;
        string label;
        #endregion

        #region Constructors
        public OwnerInfo()
        {
            ownerID = 0;
            range = DTC.RangeEnum.Floating;
            startDate = DateTime.Today;
            endDate = DateTime.Today;
            no = 0;
            label = "";
        }
        #endregion

        #region Public Methods
        public enum NextPrevEnum
        {
            Next, Previous
        }
        public OwnerInfo GetNext(NextPrevEnum np)
        {
            OwnerInfo newOwner = new OwnerInfo();
            newOwner.range = range;

            if (range == DTC.RangeEnum.Day)
            {
                if (np == NextPrevEnum.Next)
                {
                    newOwner.startDate = startDate.AddDays(1);
                    newOwner.endDate = endDate.AddDays(1);
                }
            }
            else if (range == DTC.RangeEnum.Week)
            {
                WeekInfo week = new WeekInfo(startDate);

                if (np == NextPrevEnum.Next)
                {
                    newOwner.startDate = DTC.Date.GetNextWeek(week).StartDate;
                    newOwner.endDate = DTC.Date.GetNextWeek(week).EndDate;
                }
                else if (np == NextPrevEnum.Previous)
                {
                    newOwner.startDate = DTC.Date.GetPreviousWeek(week).StartDate;
                    newOwner.endDate = DTC.Date.GetPreviousWeek(week).EndDate;
                }
            }
            else if (range == DTC.RangeEnum.Month)
            {
                MonthInfo month = new MonthInfo(startDate);

                if (np == NextPrevEnum.Next)
                {
                    newOwner.startDate = DTC.Date.GetNextMonth(month).StartDate;
                    newOwner.endDate = DTC.Date.GetNextMonth(month).EndDate;
                }
                else if (np == NextPrevEnum.Previous)
                {
                    newOwner.startDate = DTC.Date.GetPreviousMonth(month).StartDate;
                    newOwner.endDate = DTC.Date.GetPreviousMonth(month).EndDate;
                }
            }
            else if (range == DTC.RangeEnum.Quarter)
            {
                QuarterInfo quarter = new QuarterInfo(startDate);

                if (np == NextPrevEnum.Next)
                {
                    newOwner.startDate = DTC.Date.GetNextQuarter(quarter).StartDate;
                    newOwner.endDate = DTC.Date.GetNextQuarter(quarter).EndDate;
                }
                else if (np == NextPrevEnum.Previous)
                {
                    newOwner.startDate = DTC.Date.GetPreviousQuarter(quarter).StartDate;
                    newOwner.endDate = DTC.Date.GetPreviousQuarter(quarter).EndDate;
                }
            }
            else if (range == DTC.RangeEnum.Year)
            {
                YearInfo year = new YearInfo(startDate.Year);

                if (np == NextPrevEnum.Next)
                {
                    newOwner.startDate = new YearInfo(startDate.Year + 1).StartDate;
                    newOwner.endDate = new YearInfo(startDate.Year + 1).EndDate;
                }
                else if (np == NextPrevEnum.Previous)
                {
                    newOwner.startDate = new YearInfo(startDate.Year - 1).StartDate;
                    newOwner.endDate = new YearInfo(startDate.Year - 1).EndDate;
                }
            }

            return newOwner;
        }
        #endregion

        #region Public Properties
        public int OwnerID { get { return ownerID; } set { ownerID = value; }}
        public DTC.RangeEnum Range { get { return range; } set { range = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; }}
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public int NO { get { return no; } set { no = value; }}
        public string Label { get { return label; } set { label = value; }}
        #endregion
    }
}