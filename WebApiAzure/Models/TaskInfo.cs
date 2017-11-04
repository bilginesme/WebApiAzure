using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class TaskInfo : ICloneable
    {
        #region Enums
        public enum SeperatorEnum : int { None = 0, Before = 1, After = 2 }
        #endregion

        #region Private Members
        long id;
        string title;
        int projectID;
        int projectGroupID;
        long blockID;
        long segmentID;
        bool isCompleted;
        string details;
        int plannedTime;
        int realTime;
        DateTime startDate;
        DateTime endDate;
        DateTime taskDate;
        int orderActive;
        int orderGeneral;
        bool isFloating;
        bool isThing;
        bool isActive;
        bool canBeep;
        int chapterID;
        bool hasDue;
        DateTime dueDate;
        string seperatorHour;
        SeperatorEnum seperator;
        bool isPrivilaged;
        int templateID;

        Dictionary<long, CoTaskInfo> coTasks;
        #endregion

        #region Constructors
        public TaskInfo()
        {
            id = 0;
            title = "";
            hasDue = false;
            dueDate = DateTime.Today;
            taskDate = DateTime.Today;
            startDate = DateTime.Today;
            endDate = DateTime.Today;
            projectID = 0;
            projectGroupID = 0;
            blockID = 0;
            segmentID = 0;
            isCompleted = false;
            details = "";
            plannedTime = 0;
            realTime = 0;
            orderActive = 0;
            orderGeneral = 0;
            isFloating = false;
            isThing = false;
            isActive = false;
            canBeep = true;
            coTasks = new Dictionary<long, CoTaskInfo>();
            chapterID = 0;
            seperator = SeperatorEnum.None;
            seperatorHour = "00:00";
            isPrivilaged = false;
            templateID = 0;
        }
        public TaskInfo(int id, string title)
        {
            this.id = id;
            this.title = title;
        }
        #endregion

        #region Public Methods
        public int GetRemainingMinutes()
        {
            int result = 0;

            result = plannedTime - realTime;
            if (result < 0) result = 0;

            return result;
        }
        public int GetPercentage()
        {
            int percentage = 0;

            if (plannedTime > 0)
            {
                percentage = (int)(100 * (float)realTime / (float)plannedTime);
                if (percentage > 100) percentage = 100;
                if (percentage < 0) percentage = 0;
            }

            return percentage;
        }
        public float GetProjectedTime()
        {
            float result = 0;

            if (isCompleted)
            {
                result = realTime;
            }
            else
            {
                if (!isFloating)
                {
                    if (realTime <= plannedTime)
                    {
                        result = plannedTime;
                    }
                    else if (realTime > plannedTime)
                    {
                        result = realTime;
                    }
                }
                else
                {
                    result = realTime;
                }
            }

            return result;
        }
        public int GetAge()
        {
            int age = 0;

            TimeSpan ts = DateTime.Today.Subtract(startDate);
            age = (int)ts.TotalDays;
            return age;
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
        public int ProjectID
        {
            get { return projectID; }
            set { projectID = value; }
        }
        public int ProjectGroupID
        {
            get { return projectGroupID; }
            set { projectGroupID = value; }
        }
        public long BlockID
        {
            get { return blockID; }
            set { blockID = value; }
        }
        public long SegmentID
        {
            get { return segmentID; }
            set { segmentID = value; }
        }
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; }
        }
        public bool IsPrivilaged
        {
            get { return isPrivilaged; }
            set { isPrivilaged = value; }
        }
        public string Details
        {
            get { return details; }
            set { details = value; }
        }
        public int PlannedTime
        {
            get { return plannedTime; }
            set { plannedTime = value; }
        }
        public int RealTime
        {
            get { return realTime; }
            set { realTime = value; }
        }
        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }
        public DateTime TaskDate
        {
            get { return taskDate; }
            set { taskDate = value; }
        }
        public int OrderActive
        {
            get { return orderActive; }
            set { orderActive = value; }
        }
        public int OrderGeneral
        {
            get { return orderGeneral; }
            set { orderGeneral = value; }
        }
        public bool IsFloating
        {
            get { return isFloating; }
            set { isFloating = value; }
        }
        public bool IsThing
        {
            get { return isThing; }
            set { isThing = value; }
        }
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
        public bool CanBeep
        {
            get { return canBeep; }
            set { canBeep = value; }
        }
        public Dictionary<long, CoTaskInfo> CoTasks
        {
            get { return coTasks; }
            set { coTasks = value; }
        }
        public int ChapterID
        {
            get { return chapterID; }
            set { chapterID = value; }
        }
        public bool HasDue
        {
            get { return hasDue; }
            set { hasDue = value; }
        }
        public DateTime DueDate
        {
            get { return dueDate; }
            set { dueDate = value; }
        }
        public SeperatorEnum Seperator
        {
            get { return seperator; }
            set { seperator = value; }
        }
        public string SeperatorHour
        {
            get { return seperatorHour; }
            set { seperatorHour = value; }
        }
        public int TemplateID
        {
            get { return templateID; }
            set { templateID = value; }
        }
        #endregion


        object ICloneable.Clone()
        {
            // make memberwise copy
            return this.MemberwiseClone();
        }
    }
}