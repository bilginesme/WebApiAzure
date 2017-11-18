using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class GoalsOfProjectController : ApiController
    {
        [HttpGet]
        [Route("api/GoalsOfProject/{projectID}/{isOnlyRunningOnes}")]
        public IEnumerable<GoalInfo> Get(int projectID, bool isOnlyRunningOnes)
        {
            List<GoalInfo> goals = DB.Goals.GetGoalsOfProject(projectID, true, true);

            return goals;
        }

        // GET: api/GoalsOfProject/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/GoalsOfProject
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/GoalsOfProject/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/GoalsOfProject/5
        public void Delete(int id)
        {
        }
    }
}
