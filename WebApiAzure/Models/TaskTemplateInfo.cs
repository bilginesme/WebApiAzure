using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class TaskTemplateInfo
    {
        #region Private Members
        int id;
        string name;
        string details;
        #endregion

        #region Constructors
        public TaskTemplateInfo()
        {
            id = 0;
            name = "";
            details = "";
        }
        #endregion

        #region Public Properties
        public int ID { get { return id; } set { id = value; }}
        public string Name { get { return name; } set { name = value; }}
        public string Details { get { return details; } set { details = value; }}
        #endregion
    }
}