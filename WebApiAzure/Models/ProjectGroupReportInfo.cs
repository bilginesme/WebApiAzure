using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{

    public class ProjectGroupReportInfo
    {
        #region Members
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int ProjectTypeID { get; set; }
        public string ProjectTypeName { get; set; }
        public int RealTime { get; set; }
        #endregion

        #region Constructors
        public ProjectGroupReportInfo()
        {
            ID = 0;
            Name = string.Empty;
            Code = string.Empty;
            ProjectTypeID = 0;
            ProjectTypeName = string.Empty;
            RealTime = 0;
        }
        #endregion
    }
}