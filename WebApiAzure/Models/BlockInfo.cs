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
        long clusterID;
        DateTime startDate;
        DateTime endDate;
        DateTime dueDate;
        bool hasDue;
        DTC.StatusEnum status;
        int order;
        string projectCode;
        int runningGoalID;
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
            clusterID = 0;
            status = DTC.StatusEnum.Running;
            order = 0;
            projectCode = string.Empty;
            runningGoalID = 0;
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
        public long ClusterID { get { return clusterID; } set { clusterID = value; } }
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; }}
        public DateTime DueDate { get { return dueDate; } set { dueDate = value; }}
        public bool HasDue { get { return hasDue; } set { hasDue = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }}
        public int Order { get { return order; } set { order = value; }}
        public string ProjectCode { get { return projectCode; } set { projectCode = value; }}
        public int RunningGoalID { get { return runningGoalID; } set { runningGoalID = value; }}
        public int AgeDays { get { return (int)DateTime.Today.Subtract(startDate).TotalDays; } }
        #endregion

        object ICloneable.Clone() { return this.MemberwiseClone(); }
    }
}
