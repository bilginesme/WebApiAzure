using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class PeriodInfo
    {
        #region Private Members
        DateTime startDate;
        DateTime endDate;
        #endregion

        #region Constructors
        public PeriodInfo()
        {
            startDate = DateTime.Now;
            endDate = DateTime.Now;
        }
        public PeriodInfo(DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            this.endDate = endDate;
        }
        #endregion

        #region Private Methods
        private PeriodInfo GetNextPrev(DTC.RangeEnum range, int p)
        {
            PeriodInfo period = new PeriodInfo(startDate, endDate);

            if (range == DTC.RangeEnum.Day)
            {
                period.StartDate = startDate.AddDays(1 * p);
                period.EndDate = endDate.AddDays(1 * p);
            }
            else if (range == DTC.RangeEnum.Week)
            {
                period.StartDate = startDate.AddDays(7 * p);
                period.EndDate = endDate.AddDays(7 * p);
            }
            else if (range == DTC.RangeEnum.Month)
            {
                period.StartDate = startDate.AddMonths(1 * p);
                period.EndDate = endDate.AddMonths(1 * p);
            }
            else if (range == DTC.RangeEnum.Quarter)
            {
                period.StartDate = startDate.AddMonths(3 * p);
                period.EndDate = endDate.AddMonths(3 * p);
            }
            else if (range == DTC.RangeEnum.Year)
            {
                period.StartDate = startDate.AddYears(1 * p);
                period.EndDate = endDate.AddYears(1 * p);
            }
            else if (range == DTC.RangeEnum.Decade)
            {
                period.StartDate = startDate.AddYears(10 * p);
                period.EndDate = endDate.AddYears(10 * p);
            }

            return period;
        }
        #endregion

        #region Public Methods
        public PeriodInfo GetNext(DTC.RangeEnum range) { return GetNextPrev(range, 1); }
        public PeriodInfo GetPrev(DTC.RangeEnum range) { return GetNextPrev(range, -1); }

        #endregion
        #region Public Properties
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; }}
        #endregion
    }
}