using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectTypeInfo
    {
        #region Private Members
        int projectTypeID;
        string projectTypeName;
        string projectTypeCode;
        int projectTypeOrder;
        #endregion

        #region Constructors
        public ProjectTypeInfo()
        {
            projectTypeID = 0;
            projectTypeName = "";
            projectTypeCode = "";
            projectTypeOrder = 0;
        }
        public ProjectTypeInfo(int projectTypeID, string projectTypeName)
        {
            this.projectTypeID = projectTypeID;
            this.projectTypeName = projectTypeName;
        }
        #endregion

        #region Public Properties
        public int ProjectTypeID { get { return projectTypeID; } set { projectTypeID = value; }}
        public string ProjectTypeName { get { return projectTypeName; } set { projectTypeName = value; }}
        public string ProjectTypeCode { get { return projectTypeCode; } set { projectTypeCode = value; }}
        public int ProjectTypeOrder { get { return projectTypeOrder; } set { projectTypeOrder = value; }}
        #endregion
    }
}