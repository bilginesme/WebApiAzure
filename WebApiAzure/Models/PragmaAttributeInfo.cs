using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class PragmaAttributeInfo
    {
        #region Private Members
        int id;
        string name;
        int order;
        #endregion

        #region Constructors
        public PragmaAttributeInfo()
        {
            id = 0;
            name = "";
            order = 0;
        }
        public PragmaAttributeInfo(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        #endregion

        #region Public Properties
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public int Order
        {
            get { return order; }
            set { order = value; }
        }
        #endregion
    }
}