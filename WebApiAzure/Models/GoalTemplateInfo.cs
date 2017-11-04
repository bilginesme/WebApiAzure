using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class GoalTemplateInfo
    {
        #region Private Members
        int id;
        string name;
        string details;
        DTC.RangeEnum range;
        #endregion

        #region Constructors
        public GoalTemplateInfo()
        {
            id = 0;
            name = "";
            details = "";
            range = DTC.RangeEnum.Floating;
        }
        public GoalTemplateInfo(int id, string name, string details, DTC.RangeEnum range)
        {
            this.id = id;
            this.name = name;
            this.details = details;
            this.range = range;
        }
        #endregion

        #region Public Properties
        public int ID { get { return id; } set { id = value; }}
        public string Name { get { return name; } set { name = value; }}
        public string Details { get { return details; } set { details = value; }}
        public DTC.RangeEnum Range { get { return range; } set { range = value; }}
        #endregion
    }
}