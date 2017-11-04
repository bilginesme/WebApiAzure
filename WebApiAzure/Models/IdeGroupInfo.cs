using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class IdeaGroupInfo
    {
        #region Enums
        public enum TypeEnum : int
        {
            Idea = 1, List = 2
        }
        #endregion

        #region Private Members
        int id;
        string title;
        DateTime creationDate, lastUpdateDate;
        int projectID;
        int projectGroupID;
        int numIdeas;
        string details;
        bool isFocused;
        #endregion

        #region Constructors
        public IdeaGroupInfo()
        {
            id = 0;
            title = "";
            creationDate = DateTime.Today;
            lastUpdateDate = DateTime.Today;
            numIdeas = 0;
            projectID = 0;
            projectGroupID = 0;
            details = "";
            isFocused = false;
        }
        #endregion

        #region Public Methods
        public int GetPassedDays()
        {
            TimeSpan ts = DateTime.Today.Subtract(lastUpdateDate);
            return (int)ts.TotalDays;
        }
        #endregion

        #region Public Properties
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        public DateTime CreationDate
        {
            get { return creationDate; }
            set { creationDate = value; }
        }
        public DateTime LastUpdateDate
        {
            get { return lastUpdateDate; }
            set { lastUpdateDate = value; }
        }
        public int ProjectID
        {
            get { return projectID; }
            set { projectID = value; }
        }
        public int ProjectGroupID
        {
            get { return projectGroupID; }
            set { projectGroupID = value; }
        }
        public int NumIdeas
        {
            get { return numIdeas; }
            set { numIdeas = value; }
        }
        public string Details
        {
            get { return details; }
            set { details = value; }
        }
        public bool IsFocused
        {
            get { return isFocused; }
            set { isFocused = value; }
        }
        #endregion
    }
}