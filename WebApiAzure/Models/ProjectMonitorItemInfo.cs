using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectMonitorItemInfo
    {
        #region Public Members
        public int ProjectID { get; set; }
        public DateTime TheDate { get; set; }
        public int NumMinutes { get; set; }
        #endregion

        #region Constructors
        public ProjectMonitorItemInfo()
        {
            ProjectID = 0;
            TheDate = DateTime.MinValue;
            NumMinutes = 0;
        }
        #endregion
    }
}   