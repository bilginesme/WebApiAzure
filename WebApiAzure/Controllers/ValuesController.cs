using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        // GET api/values
       

        public IEnumerable<ProjectInfo> Get()
        {
            List<ProjectInfo> projects = DB.GetProjects();

            /*
            List<ProjectInfo> projects = new List<ProjectInfo>();

            projects.Add(new ProjectInfo(1, "Xar", "Xarama"));
            projects.Add(new ProjectInfo(2, "Sum", "Sumatra"));
            projects.Add(new ProjectInfo(3, "Ghe", "The Ghetto"));

            //return new string[] { "value1", "value2", "value three" };

            */

            return projects;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
