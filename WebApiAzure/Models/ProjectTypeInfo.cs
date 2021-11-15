using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectTypeInfo
    {
        #region Private Members
        int id;
        string name;
        string code;
        int order;
        #endregion

        #region Constructors
        public ProjectTypeInfo()
        {
            id = 0;
            name = "";
            code = "";
            order = 0;
        }
        public ProjectTypeInfo(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        #endregion

        #region Public Properties
        public int ID { get { return id; } set { id = value; }}
        public string Name { get { return name; } set { name = value; }}
        public string Code { get { return code; } set { code = value; }}
        public int Order { get { return order; } set { order = value; }}
        #endregion
    }
}