using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class PragmaInfo
    {
        #region Private Members
        int id;
        string name;
        bool isActive;
        Dictionary<int, PragmaAttributeInfo> attributes;
        ProjectInfo project;
        ProjectGroupInfo projectGroup;
        #endregion

        #region Constructors
        public PragmaInfo()
        {
            id = 0;
            name = "";
            isActive = true;
            attributes = new Dictionary<int, PragmaAttributeInfo>();
            project = new ProjectInfo();
            projectGroup = new ProjectGroupInfo();
        }
        public PragmaInfo(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        #endregion

        #region Public Methods
        public string GetSmartProjectCode(bool isShort)
        {
            string result = "";

            if (project != null && projectGroup != null)
            {
                if (project.ID > 0)
                {
                    ProjectGroupInfo pG = DB.ProjectGroups.GetProjectGroup(project.ProjectGroupID);
                    result = project.GetSmartCode(pG, isShort);
                }
                else if (projectGroup.ID > 0)
                {
                    result = projectGroup.Code;
                }
            }

            return result;
        }
        #endregion

        #region Public Properties
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
        public Dictionary<int, PragmaAttributeInfo> Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }
        public ProjectInfo Project
        {
            get { return project; }
            set { project = value; }
        }
        public ProjectGroupInfo ProjectGroup
        {
            get { return projectGroup; }
            set { projectGroup = value; }
        }
        #endregion
    }
}