using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WebApiAzure
{
    public class BlockEnhancedInfo : BlockInfo
    {
        #region Private Members
        int numSegmentsTotal;
        int numSegmentsCompleted;
        int totalMinutes;
        #endregion

        #region Constructors
        public BlockEnhancedInfo()
        {
            numSegmentsTotal = 0;
            numSegmentsCompleted = 0;
            totalMinutes = 0;
        }
        #endregion

        #region Public Methods
       
        #endregion

        #region Public Properties
        public int NumSegmentsTotal { get { return numSegmentsTotal; } set { numSegmentsTotal = value; }}
        public int NumSegmentsCompleted { get { return numSegmentsCompleted; } set { numSegmentsCompleted = value; } }
        public int TotalMinutes { get { return totalMinutes; } set { totalMinutes = value; } }
        #endregion

    }
}
