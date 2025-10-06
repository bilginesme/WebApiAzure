using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class CoTaskInfo
    {
        #region Private Members
        long id;
        long taskID;
        string title;
        int projectGroupID;
        int projectID;
        #endregion

        #region Constructors
        public CoTaskInfo()
        {
            id = 0;
            taskID = 0;
            title = "";
            projectID = 0;
        }
        public CoTaskInfo(CoTaskInfo tCoTask)
        {
            id = tCoTask.ID;
            taskID = tCoTask.TaskID;
            title = tCoTask.Title;
            projectID = tCoTask.ProjectID;
        }
        public CoTaskInfo(int coTaskID, string title)
        {
            this.id = coTaskID;
            this.title = title;
        }
        #endregion
         
        #region Public Methods
        
        #endregion

        #region Public Properties
        public long TaskID { get { return taskID; } set { taskID = value; } }
        public long ID { get { return id; } set { id = value; }}
        public string Title { get { return title; } set { title = value; }}
        public int ProjectID { get { return projectID; } set { projectID = value; } }
        public int ProjectGroupID { get { return projectGroupID; } set { projectGroupID = value; } }
        #endregion
    }
}