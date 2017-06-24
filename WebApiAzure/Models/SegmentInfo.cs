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
        int blockID;
        DTC.StatusEnum status;
        #endregion

        #region Constructors
        public SegmentInfo()
        {
            id = 0;
            title = string.Empty;
            details = string.Empty;
            blockID = 0;
            status = DTC.StatusEnum.Running;
        }
        public SegmentInfo(int id, string title)
        {
            this.id = id;
            this.title = title;
        } 
        #endregion

        #region Public Methods
        #endregion

        #region Public Properties
        public int ID { get { return id; } set { id = value; }}
        public string Title { get { return title; } set { title = value; }}
        public string Details { get { return details; } set { details = value; }}
        public int BlockID { get { return blockID; } set { blockID = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }}
        #endregion

        object ICloneable.Clone()
        {
            // make memberwise copy
            return this.MemberwiseClone();
        }
    }
}
