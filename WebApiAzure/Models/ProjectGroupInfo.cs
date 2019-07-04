using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{

    public class ProjectGroupInfo
    {
        #region Members
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int ProjectTypeID { get; set; }
        public DTC.StatusEnum Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Details { get; set; }
        #endregion

        #region Constructors
        public ProjectGroupInfo()
        {
            ID = 0;
            Name = string.Empty;
            Code = string.Empty;
            ProjectTypeID = 0;
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MaxValue;
            Status = DTC.StatusEnum.Running;
            Details = string.Empty;
        }
        #endregion
    }
}