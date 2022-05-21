using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class IdeaInfo
    {
        #region Private Members
        public long ID { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public int Order { get; set; }
        public int IdeaGroupID { get; set; }
        public string Details { get; set; }
        public DTC.SizeEnum Impact { get; set; }
        public DTC.StatusEnum Status { get; set; }
        public int InnovativePoint { get; set; }
        public bool IsFocused { get; set; }
        public bool IsActionable { get; set; }
        public DateTime ActionDueDate { get; set; }
        public string ProjectNameLazy { get; set; }
        public string ImageNameLazy { get; set; }
        #endregion

        #region Constructors
        public IdeaInfo()
        {
            ID = 0;
            Title = string.Empty;
            CreationDate = DateTime.Today;
            IdeaGroupID = 0;
            Order = 0;
            Details = string.Empty;
            Impact = DTC.SizeEnum.Zero;
            Status = DTC.StatusEnum.Running;
            InnovativePoint = 0;
            IsFocused = false;
            IsActionable = false;
            ActionDueDate = DateTime.Today;
            ProjectNameLazy = string.Empty;
            ImageNameLazy = string.Empty;
        }
        #endregion

        #region Public Methods
        public bool HasDetails()
        {
            if (Details.Length > 0) return true;
            else return false;
        }
        public int GetDaysForAction(DateTime date)
        {
            int result = 0;

            TimeSpan ts = ActionDueDate.Subtract(date);
            result = (int)Math.Round(ts.TotalDays);

            return result;
        }
        #endregion
    }
}