using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectGroupInfo
    {
        #region Private Members
        int id;
        string name;
        string code;
        int projectTypeID;
        bool isActive;
        bool isActionable;
        bool isCompletable;
        bool isCompleted;
        DateTime startDate;
        DateTime dueDate;
        DateTime endDate;
        DTC.StatusEnum status;
        #endregion

        #region Constructors
        public ProjectGroupInfo()
        {
            id = 0;
            name = "";
            code = "";
            projectTypeID = 0;
            isActive = true;
            isCompletable = true;
            isCompleted = false;
            isActionable = true;
            startDate = DateTime.MinValue;
            dueDate = DateTime.MaxValue;
            endDate = DateTime.MaxValue;
            status = DTC.StatusEnum.Running;
        }
        public ProjectGroupInfo(int projectGroupID, string projectGroupName)
        {
            this.id = projectGroupID;
            this.name = projectGroupName;
        }
        #endregion

        #region Public Properties
        public int ID { get { return id; } set { id = value; }}
        public string Name { get { return name; } set { name = value; }}
        public string Code { get { return code; } set { code = value; }}
        public int ProjectTypeID { get { return projectTypeID; } set { projectTypeID = value; }}
        public bool IsActive { get { return isActive; } set { isActive = value; }}
        public bool IsCompletable { get { return isCompletable; } set { isCompletable = value; }}
        public bool IsCompleted { get { return isCompleted; } set { isCompleted = value; }}
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime DueDate { get { return dueDate; } set { dueDate = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }}
        public bool IsActionable { get { return isActionable; } set { isActionable = value; }}
        #endregion
    }
}