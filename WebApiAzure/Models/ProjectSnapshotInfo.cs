using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectSnapshotInfo
    {
        public int ProjectID { get; set; }
        public DTC.StatusEnum Status { get; set; }
        public DTC.RankEnum Rank { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string ProjectImgName { get; set; }
        public string ProjectImageThumb { get; set; }
        public string ProjectColor { get; set; }
        public string ProjectGroupCode { get; set; }
        public float RealTime { get; set; }
        public float EternalTotalTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int NumDaysRemaining { get; set; }
        public int PercentCompleted { get; set; }
        public float CompletionRate { get; set; }
        public float HoursNeededToComplete { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public DateTime EstimatedCompletionDateBasedOnLast30Days { get; set; }
        public float WorkPerDayNeededForDueDate { get; set; }
        public float W0 { get; set; }
        public float W1 { get; set; }
        public float W2 { get; set; }
        public float W3 { get; set; }
        public string Details { get; set; }

        public ProjectSnapshotInfo()
        {

        }
    }
}