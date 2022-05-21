using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebApiAzure.Models
{
    public class SubProjectInfo
    {
        public long ID { get; set; }
        public string Name { get; set;}
        public int ProjectID { get; set;}
        public bool IsCompleted { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Details { get; set; }
        public int NumClustersTotal { get; set; }
        public float HoursNeeded { get; set; }
        public float HoursSpent { get; set; }
        public float RatioSpentNeeded { get; set; }
        public float Contribution { get; set; }
        public float ContributionMax { get; set; }
        public float ContributionPercentage { get; set; }

        public SubProjectInfo()
        {

        }
    }
}