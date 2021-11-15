using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ClusterInfo
    {
        #region Private Members
        long id;
        string title;
        float hoursNeeded, hoursSpent;
        int projectID;
        string details;
        bool isCompleted;
        DateTime startDate, endDate;
        float ratioSpentNeeded, contribution, contributionMax;
        #endregion

        #region Constructors
        public ClusterInfo()
        {
            id = 0;
            title = string.Empty;
            hoursNeeded = 0;
            projectID = 0;
            details = string.Empty;
            startDate = DateTime.Today;
            endDate = DateTime.Today;
            isCompleted = false;
            ratioSpentNeeded = 0;
            contribution = 0;
            contributionMax = 0;
        }
        #endregion

        #region Public Methods
        public float GetRatioSpentNeeded()
        {
            float percentage = 0;

            if(isCompleted)
            {
                percentage = 1.0f;
            }
            else
            {
                if (hoursNeeded > 0)
                    percentage = hoursSpent / hoursNeeded;

                if (percentage > 1)
                    percentage = 1;
            }

            return percentage;
        }
        public float GetEffectiveHoursNeeded()
        {
            if (isCompleted)
                return hoursSpent;
            else
                return hoursNeeded;
        }
        #endregion

        #region Public Properties
        public long ID { get { return id; } set { id = value; }}
        public string Title { get { return title; } set { title = value; }}
        public float HoursNeeded { get { return hoursNeeded; } set { hoursNeeded = value; } }
        public float HoursSpent { get { return hoursSpent; } set { hoursSpent = value; } }
        public int ProjectID { get { return projectID; } set { projectID = value; } }
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; } }
        public string Details { get { return details; } set { details = value; }}
        public bool IsCompleted { get { return isCompleted; } set { isCompleted = value; }}
        public float RatioSpentNeeded { get { return ratioSpentNeeded; } set { ratioSpentNeeded = value; } }
        public float Contribution { get { return contribution; } set { contribution = value; } }
        public float ContributionMax { get { return contributionMax; } set { contributionMax = value; } }
        #endregion
    }
}