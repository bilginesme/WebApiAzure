using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ReportItemInfo
    {
        #region Public Members
        public int ID { get; set; }
        public string Label { get; set; }
        public double Hours { get; set; }
        #endregion

        #region Constructors
        public ReportItemInfo()
        {
            ID = 0;
            Label = string.Empty;
            Hours = 0;
        }
        #endregion
    }
}   