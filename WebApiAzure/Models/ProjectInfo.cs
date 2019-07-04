using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectInfo : ICloneable
    {
        public enum ShowHowManyTasksEnum { All = -1, None = 0, Three = 3, Five = 5, Ten = 10 }

        #region Members
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ProjectImgName { get; set; }
        public string Details { get; set; }
        public int ProjectGroupID { get; set; }
        public bool IsCompletable { get; set; }
        public bool IsActionable { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime EndDate { get; set; }
        public DTC.StatusEnum Status { get; set; }
        public DTC.RankEnum Rank { get; set; }
        public int Order { get; set; }
        public ShowHowManyTasksEnum ShowHowManyTasks { get; set; }
        public DTC.RangeEnum MonitoringFrequency { get; set; }
        public string SmartCode { get; set; }
        public float CompletionRate { get; set; }
        #endregion

        #region Constructors
        public ProjectInfo()
        {
            ID = 0;
            Name = "";
            Code = "";
            ProjectImgName = string.Empty;
            Details = "";
            ProjectGroupID = 0;
            IsCompletable = true;
            IsActionable = false;
            StartDate = DateTime.MinValue;
            DueDate = DateTime.MaxValue;
            EndDate = DateTime.MaxValue;
            Status = DTC.StatusEnum.Running;
            Rank = DTC.RankEnum.NoRank;
            Order = 0;
            ShowHowManyTasks = ShowHowManyTasksEnum.All;
            MonitoringFrequency = DTC.RangeEnum.Month;
            CompletionRate = 0;
        }
        public ProjectInfo(int id, string name)
        {
            ID = id;
            Name = name;
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
            if (ID > 0)
            {
                if (isShort)
                    return projectGroup.Code + "▫" + Code;
                else
                    return projectGroup.Code + " | " + Name;
            }
            else
            {
                return "";
            }
        }
        public int GetTotalDays()
        {
            TimeSpan ts = DueDate.Subtract(StartDate);
            return (int)ts.TotalDays;
        }
        public int GetNumberOfDaysPassed()
        {
            TimeSpan ts = DateTime.Today.Subtract(StartDate);
            return (int)ts.TotalDays;
        }
        public int GetNumberOfDaysLeft()
        {
            if (IsCompletable)
            {
                TimeSpan ts = DueDate.Subtract(DateTime.Today);
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

        object ICloneable.Clone() { return this.MemberwiseClone();}
    }
}