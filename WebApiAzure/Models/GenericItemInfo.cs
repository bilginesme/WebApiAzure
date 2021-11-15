using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class GenericItemInfo
    {
        #region Public Members
        public int ID { get; set; }
        public string Name { get; set; }
        #endregion

        #region Constructors
        public GenericItemInfo()
        {
            ID = 0;
            Name = string.Empty;
        }
        #endregion
    }
}   