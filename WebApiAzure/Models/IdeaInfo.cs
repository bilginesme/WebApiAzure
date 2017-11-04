using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class IdeaInfo
    {
        #region Private Members
        int id;
        string title;
        DateTime creationDate;
        int ideaGroupID;
        int order;
        string details;
        DTC.SizeEnum impact;
        DTC.StatusEnum status;
        int innovativePoint;
        bool isFocused;
        bool isActionable;
        DateTime actionDueDate;
        #endregion

        #region Constructors
        public IdeaInfo()
        {
            id = 0;
            title = "";
            creationDate = DateTime.Today;
            ideaGroupID = 0;
            order = 0;
            details = "";
            impact = DTC.SizeEnum.Zero;
            status = DTC.StatusEnum.Running;
            innovativePoint = 0;
            isFocused = false;
            isActionable = false;
            actionDueDate = DateTime.Today;
        }
        #endregion

        #region Public Methods
        public bool HasDetails()
        {
            if (details.Length > 0) return true;
            else return false;
        }
        public int GetDaysForAction(DateTime date)
        {
            int result = 0;

            TimeSpan ts = actionDueDate.Subtract(date);
            result = (int)Math.Round(ts.TotalDays);

            return result;
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
        public int Order
        {
            get { return order; }
            set { order = value; }
        }
        public int IdeaGroupID
        {
            get { return ideaGroupID; }
            set { ideaGroupID = value; }
        }
        public string Details
        {
            get { return details; }
            set { details = value; }
        }
        public DTC.SizeEnum Impact
        {
            get { return impact; }
            set { impact = value; }
        }
        public DTC.StatusEnum Status
        {
            get { return status; }
            set { status = value; }
        }
        public int InnovativePoint
        {
            get { return innovativePoint; }
            set { innovativePoint = value; }
        }
        public bool IsFocused
        {
            get { return isFocused; }
            set { isFocused = value; }
        }
        public bool IsActionable
        {
            get { return isActionable; }
            set { isActionable = value; }
        }
        public DateTime ActionDueDate
        {
            get { return actionDueDate; }
            set { actionDueDate = value; }
        }
        #endregion
    }
}