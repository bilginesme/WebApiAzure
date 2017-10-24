using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WebApiAzure
{
    public class SegmentEnhancedInfo : SegmentInfo
    {
        #region Private Members
        string projectName;
        string blockTitle;
        #endregion

        #region Constructors
        public SegmentEnhancedInfo()
        {
            projectName = string.Empty;
            blockTitle = string.Empty;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Public Properties
        public string ProjectName { get { return projectName; } set { projectName = value; } }
        public string BlockTitle { get { return blockTitle; } set { blockTitle = value; } }
        #endregion
    }
}
