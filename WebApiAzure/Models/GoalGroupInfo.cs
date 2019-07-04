using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class GoalGroupInfo
    {
        #region Members
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Order { get; set; }
        public DTC.SizeEnum Leverage { get; set; }
        #endregion

        #region Constructors
        public GoalGroupInfo()
        {
            ID = 0;
            Name = string.Empty;
            Code = string.Empty;
            Order = 0;
            Leverage = DTC.SizeEnum.Zero;
        }
        public GoalGroupInfo(int id, string name)
        {
            ID = id;
            Name = name;
        }
        #endregion
    }
}