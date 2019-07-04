using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class TaskInfo : ICloneable
    {
        public enum SeperatorEnum : int { None = 0, Before = 1, After = 2 }

        #region Members
        public long ID { get; set; }
        public string Title { get; set; }
        public int ProjectID { get; set; }
        public int ProjectGroupID { get; set; }
        public long BlockID { get; set; }
        public long SegmentID { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPrivilaged { get; set; }
        public string Details { get; set; }
        public int PlannedTime { get; set; }
        public int RealTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime TaskDate { get; set; }
        public int OrderActive { get; set; }
        public int OrderGeneral { get; set; }
        public bool IsFloating { get; set; }
        public bool IsThing { get; set; }
        public bool CanBeep { get; set; }
        public SeperatorEnum Seperator { get; set; }
        public string SeperatorHour { get; set; }
        public int TemplateID { get; set; }
        #endregion

        #region Constructors
        public TaskInfo()
        {
            ID = 0;
            Title = string.Empty;
            TaskDate = DateTime.Today;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            ProjectID = 0;
            ProjectGroupID = 0;
            BlockID = 0;
            SegmentID = 0;
            IsCompleted = false;
            Details = string.Empty;
            PlannedTime = 0;
            RealTime = 0;
            OrderActive = 0;
            OrderGeneral = 0;
            IsFloating = false;
            IsThing = false;
            CanBeep = true;
            Seperator = SeperatorEnum.None;
            SeperatorHour = "00:00";
            IsPrivilaged = false;
            TemplateID = 0;
        }
        public TaskInfo(int id, string title)
        {
            ID = id;
            Title = title;
        }
        #endregion

        #region Public Methods
        public int GetRemainingMinutes()
        {
            int result = 0;

            result = PlannedTime - RealTime;
            if (result < 0) result = 0;

            return result;
        }
        public int GetPercentage()
        {
            int percentage = 0;

            if (PlannedTime > 0)
            {
                percentage = (int)(100 * (float)RealTime / (float)PlannedTime);
                if (percentage > 100) percentage = 100;
                if (percentage < 0) percentage = 0;
            }

            return percentage;
        }
        public float GetProjectedTime()
        {
            float result = 0;

            if (IsCompleted)
            {
                result = RealTime;
            }
            else
            {
                if (!IsFloating)
                {
                    if (RealTime <= PlannedTime)
                    {
                        result = PlannedTime;
                    }
                    else if (RealTime > PlannedTime)
                    {
                        result = RealTime;
                    }
                }
                else
                {
                    result = RealTime;
                }
            }

            return result;
        }
        public int GetAge()
        {
            int age = 0;

            TimeSpan ts = DateTime.Today.Subtract(StartDate);
            age = (int)ts.TotalDays;
            return age;
        }
        #endregion

        object ICloneable.Clone()
        {
            // make memberwise copy
            return this.MemberwiseClone();
        }
    }
}