using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WebApiAzure
{
    public class BlockInfo : ICloneable
    {
        #region Private Members
        long id;        
        string title;
        string details;
        int projectID;
        bool hasDue;
        DateTime startDate;
        DateTime endDate;
        DateTime dueDate;
        DTC.StatusEnum status;
        #endregion

        #region Constructors
        public BlockInfo()
        {
            id = 0;
            title = string.Empty;
            details = string.Empty;
            startDate = DateTime.Today;
            endDate = DateTime.Today;
            dueDate = DateTime.Today;
            hasDue = false;
            projectID = 0;
            status = DTC.StatusEnum.Running;
        }
        public BlockInfo(int id, string title)
        {
            this.id = id;
            this.title = title;
        } 
        #endregion

        #region Public Methods
        public int GetNumberOfDaysLeft()
        {
            if (hasDue)
            {
                TimeSpan ts = dueDate.Subtract(DateTime.Today);
                return (int)ts.TotalDays;
            }
            else
            {
                return int.MaxValue;
            }
        }
        #endregion

        #region Public Properties
        public long ID { get { return id; } set { id = value; }}
        public string Title { get { return title; } set { title = value; }}
        public string Details { get { return details; } set { details = value; }}
        public int ProjectID { get { return projectID; } set { projectID = value; }}
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; }}
        public DateTime DueDate { get { return dueDate; } set { dueDate = value; }}
        public bool HasDue { get { return hasDue; } set { hasDue = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }}
        #endregion

        object ICloneable.Clone()
        {
            // make memberwise copy
            return this.MemberwiseClone();
        }
    }
}
