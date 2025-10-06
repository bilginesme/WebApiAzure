using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectLogInfo
    {
        public long ID { get; set; }
        public long ProjectID { get; set; }
        public float Performance { get; set; }
        public DateTime TheDate { get; set; }

        public ProjectLogInfo()
        {
            ID = 0;
            ProjectID = 0;
            Performance = 0;
            TheDate = DateTime.Now;
        }
    }
}   