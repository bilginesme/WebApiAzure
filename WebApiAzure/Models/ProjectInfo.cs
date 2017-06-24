using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectInfo
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public ProjectInfo(int ID, string Code, string Name)
        {
            this.ID = ID;
            this.Name = Name;
            this.Code = Code;
        }
    }
}