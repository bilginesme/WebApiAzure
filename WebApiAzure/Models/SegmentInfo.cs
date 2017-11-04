using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WebApiAzure
{
    public class SegmentInfo : ICloneable
    {
        #region Private Members
        long id;
        string title;
        string details;
        long blockID;
        DateTime startDate;
        DateTime endDate;
        DateTime dueDate;
        bool hasDue;
        DTC.SizeEnum size;
        DTC.StatusEnum status;
        int order;
        int numTodosAll, numTodosComplete;
        int chapterID;
        string projectCode;
        int runningGoalID;
        #endregion

        #region Constructors
        public SegmentInfo()
        {
            id = 0;
            title = string.Empty;
            details = string.Empty;
            startDate = DateTime.Today;
            endDate = DateTime.Today;
            dueDate = DateTime.Today;
            hasDue = false;
            blockID = 0;
            size = DTC.SizeEnum.Medium;
            status = DTC.StatusEnum.Running;
            order = 0;
            numTodosAll = numTodosComplete = 0;
            chapterID = 0;
            projectCode = string.Empty;
            runningGoalID = 0;
        }
        public SegmentInfo(long id, string title)
        {
            this.id = id;
            this.title = title;
        }
        #endregion

        #region Public Methods
        public float GetDaysToDue(DateTime theDate)
        {
            TimeSpan ts = dueDate.Subtract(theDate);
            return (float)ts.Days;
        }
        public float GetCompleteness()
        {
            float result = 0;
            if (status != DTC.StatusEnum.Running) result = GetSize();
            else
            {
                if (numTodosAll > 0) result = GetSize() * (float)numTodosComplete / (float)numTodosAll;
            }
            return result;
        }
        public float GetPerformance()
        {
            float result = 0;
            if (status != DTC.StatusEnum.Running) result = 100;
            else
            {
                if (numTodosAll > 0) result = 100 * (float)numTodosComplete / (float)numTodosAll;
            }
            return result;
        }
        public float GetSize()
        {
            return (float)((int)size);
        }
        public int AgeDays { get { return (int)DateTime.Today.Subtract(startDate).TotalDays; } }
        public string AgeDaysString
        {
            get
            {
                string strAge = string.Empty;

                if (AgeDays >= 365)
                {
                    strAge = DTC.Format1((decimal)AgeDays / 365M) + " years";
                }
                else if (AgeDays > 30)
                {
                    strAge = DTC.Format1((decimal)AgeDays / 30) + " months";
                }
                else if (AgeDays > 7)
                {
                    strAge = DTC.Format1((decimal)AgeDays / 7) + " weeks";
                }
                else if (AgeDays > 1)
                {
                    strAge = AgeDays + " days";
                }

                return strAge;
            }
        }
        #endregion

        #region Public Properties
        public long ID { get { return id; } set { id = value; }}
        public string Title { get { return title; } set { title = value; }}
        public string Details { get { return details; } set { details = value; }}
        public long BlockID { get { return blockID; } set { blockID = value; }}
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; }}
        public DateTime DueDate { get { return dueDate; } set { dueDate = value; }}
        public bool HasDue { get { return hasDue; } set { hasDue = value; }}
        public DTC.SizeEnum Size { get { return size; } set { size = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }}
        public int Order { get { return order; } set { order = value; }}
        public int NumTodosAll { get { return numTodosAll; } set { numTodosAll = value; }}
        public int NumTodosComplete { get { return numTodosComplete; } set { numTodosComplete = value; }}
        public int ChapterID { get { return chapterID; } set { chapterID = value; }}
        public string ProjectCode { get { return projectCode; } set { projectCode = value; }}
        public int RunningGoalID { get { return runningGoalID; } set { runningGoalID = value; }}
        #endregion

        object ICloneable.Clone() { return this.MemberwiseClone();}
    }
}
