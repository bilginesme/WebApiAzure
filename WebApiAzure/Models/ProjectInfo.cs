using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectInfo : ICloneable
    {
        #region Enums
        public enum ShowHowManyTasksEnum { All = -1, None = 0, Three = 3, Five = 5, Ten = 10 }
        #endregion

        #region Private Members
        int id;
        string name;
        string code;
        string projectImgName;
        string details;
        int projectGroupID;
        bool isActive;
        bool isCompletable;
        bool isActionable;
        DateTime startDate;
        DateTime dueDate;
        DateTime endDate;
        DTC.StatusEnum status;
        DTC.RankEnum rank;
        int order;
        ShowHowManyTasksEnum showHowManyTasks;
        DTC.RangeEnum monitoringFrequency;
        string smartCode;
        #endregion

        #region Constructors
        public ProjectInfo()
        {
            id = 0;
            name = "";
            code = "";
            projectImgName = string.Empty;
            details = "";
            projectGroupID = 0;
            isActive = true;
            isCompletable = true;
            isActionable = false;
            startDate = DateTime.MinValue;
            dueDate = DateTime.MaxValue;
            endDate = DateTime.MaxValue;
            status = DTC.StatusEnum.Running;
            rank = DTC.RankEnum.NoRank;
            order = 0;
            showHowManyTasks = ShowHowManyTasksEnum.All;
            monitoringFrequency = DTC.RangeEnum.Month;
        }
        public ProjectInfo(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the smart code as 
        /// if isShort --> XAR : XP
        /// else       --> XAR : Xposed
        /// </summary>
        /// <returns>A smart code</returns>
        public string GetSmartCode(ProjectGroupInfo projectGroup, bool isShort)
        {
            if (id > 0)
            {
                if (isShort)
                    return projectGroup.Code + "▫" + code;
                else
                    return projectGroup.Code + " | " + name;
            }
            else
            {
                return "";
            }
        }
        public int GetTotalDays()
        {
            TimeSpan ts = dueDate.Subtract(startDate);
            return (int)ts.TotalDays;
        }
        public int GetNumberOfDaysPassed()
        {
            TimeSpan ts = DateTime.Today.Subtract(startDate);
            return (int)ts.TotalDays;
        }
        public int GetNumberOfDaysLeft()
        {
            if (isCompletable)
            {
                TimeSpan ts = dueDate.Subtract(DateTime.Today);
                return (int)ts.TotalDays;
            }
            else
            {
                return int.MaxValue;
            }
        }
        public float GetShouldBePerformance()
        {
            float result = 100 * (float)GetNumberOfDaysPassed() / (float)GetTotalDays();
            if (result > 100) result = 100;
            return result;
        }
        #endregion

        #region Public Properties
        public int ID { get { return id; } set { id = value; }}
        public string Name { get { return name; } set { name = value; }}
        public string Code { get { return code; } set { code = value; }}
        public string ProjectImgName { get { return projectImgName; } set { projectImgName = value; } }
        public string Details { get { return details; } set { details = value; }}
        public int ProjectGroupID { get { return projectGroupID; } set { projectGroupID = value; }}
        public bool IsActive { get { return isActive; } set { isActive = value; }}
        public bool IsCompletable { get { return isCompletable; } set { isCompletable = value; }}
        public bool IsActionable { get { return isActionable; } set { isActionable = value; }}
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime DueDate { get { return dueDate; } set { dueDate = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }}
        public DTC.RankEnum Rank { get { return rank; } set { rank = value; } }
        public int Order { get { return order; } set { order = value; }}
        public ShowHowManyTasksEnum ShowHowManyTasks { get { return showHowManyTasks; } set { showHowManyTasks = value; }}
        public DTC.RangeEnum MonitoringFrequency { get { return monitoringFrequency; } set { monitoringFrequency = value; }}
        public string SmartCode { get { return smartCode; } set { smartCode = value; } }
        #endregion

        object ICloneable.Clone() { return this.MemberwiseClone();}
    }
}