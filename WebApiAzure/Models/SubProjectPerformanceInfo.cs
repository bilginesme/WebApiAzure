using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebApiAzure.Models
{
    public class SubProjectPerformanceInfo
    {
        public int NumClustersTotal { get; set; }
        public int NumClustersCompleted { get; set; }
        public int NumBlocksTotal { get; set; }
        public int NumBlocksCompleted { get; set; }
        public float HoursNeeded { get; set; }
        public float HoursSpent { get; set; }
        public float HoursSpentInRealLife { get; set; }
        public float PercentageCompleted { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }

        public SubProjectPerformanceInfo()
        {

        }
    }
}