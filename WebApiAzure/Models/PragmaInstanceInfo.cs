using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class PragmaInstanceInfo
    {
        #region Private Members
        int id;
        DateTime date;
        PragmaInfo pragma;
        Dictionary<int, string> values;
        ProjectInfo project;
        ProjectGroupInfo projectGroup;
        #endregion

        #region Constructors
        public PragmaInstanceInfo(PragmaInfo pragma)
        {
            this.pragma = pragma;
            id = 0;
            date = DateTime.Today;
            project = pragma.Project;
            projectGroup = pragma.ProjectGroup;
            CreateValuesMatix();
        }

        /// <summary>
        /// Only use for the instances that the pragma is unknown
        /// </summary>
        //public PragmaInstanceInfo()
        //{
        //    pragma = new PragmaInfo();
        //    id = 0;
        //    date = DateTime.Today;
        //    project = new ProjectInfo();
        //    projectGroup = new ProjectGroupInfo();
        //    values = new Dictionary<int, string>();
        //} 
        #endregion

        #region Private Methods
        private void CreateValuesMatix()
        {
            values = new Dictionary<int, string>();
            foreach (KeyValuePair<int, PragmaAttributeInfo> pair in pragma.Attributes)
            {
                values.Add(pair.Key, "");
            }
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
        public string GetSummary()
        {
            string result = "";

            result = pragma.Name;

            foreach (string str in values.Values)
            {
                result += " | " + str;
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
        public PragmaInfo Pragma
        {
            get { return pragma; }
        }
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }
        public Dictionary<int, string> Values
        {
            get { return values; }
            set { values = value; }
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