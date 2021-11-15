using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
 

namespace WebApiAzure.Controllers
{
    public class RealHoursController : ApiController
    {
        [Route("api/RealHours/")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [Route("api/RealHours/{projectID}/{isIncludeCoTasks}")]
        public float Get(int projectID, bool isIncludeCoTasks)
        {
            float result = DB.Tasks.GetTotalHoursIncludingCoTasks(projectID, isIncludeCoTasks);

            return result;
        }

        // POST: api/RealHours
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/RealHours/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/RealHours/5
        public void Delete(int id)
        {
        }
    }
}
