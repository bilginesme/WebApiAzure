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
        #region Members
        public long ID { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public int ProjectID { get; set; }
        public int ProjectGroupID { get; set; }
        public string ProjectLabelShort { get; set; }
        public string ProjectLabelLong { get; set; }
        public int NumIdeas { get; set; }
        public string Details { get; set; }
        public bool IsFocused { get; set; }
        #endregion

        #region Constructors
        public IdeaGroupInfo()
        {
            ID = 0;
            Title = "";
            CreationDate = DateTime.Today;
            LastUpdateDate = DateTime.Today;
            NumIdeas = 0;
            ProjectID = 0;
            ProjectGroupID = 0;
            ProjectLabelShort = string.Empty;
            ProjectLabelLong = string.Empty;
            Details = "";
            IsFocused = false;
        }
        #endregion

        #region Public Methods
        public int GetPassedDays()
        {
            TimeSpan ts = DateTime.Today.Subtract(LastUpdateDate);
            return (int)ts.TotalDays;
        }
        #endregion
    }
}