using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WebApiAzure
{
    public class SegmentInfo : ICloneable
    {
        #region Private Members
        int id;        
        string title;
        string details;
        long blockID;
        DTC.StatusEnum status;
        DateTime startDate;
        #endregion

        #region Constructors
        public SegmentInfo()
        {
            id = 0;
            title = string.Empty;
            details = string.Empty;
            blockID = 0;
            status = DTC.StatusEnum.Running;
            startDate = DateTime.Today;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Public Properties
        public int ID { get { return id; } set { id = value; }}
        public string Title { get { return title; } set { title = value; }}
        public string Details { get { return details; } set { details = value; }}
        public long BlockID { get { return blockID; } set { blockID = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }}
        public DateTime StartDate { get { return startDate; } set { startDate = value; } }
        public int AgeDays { get { return (int)DateTime.Today.Subtract(startDate).TotalDays; } }
        #endregion

        object ICloneable.Clone()
        {
            // make memberwise copy
            return this.MemberwiseClone();
        }
    }
}
