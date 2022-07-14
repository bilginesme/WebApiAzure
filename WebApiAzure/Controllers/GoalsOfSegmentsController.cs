using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class GoalsOfSegmentsController : ApiController
    {

        [HttpGet]
        [Route("api/GoalsOfSegments/{projectID}/{isOnlyRunningOnes}/{getPresentValues}")]
        public IEnumerable<GoalInfo> Get(int projectID, bool isOnlyRunningOnes, bool getPresentValues)
        {
            List<GoalInfo> goals = DB.Goals.GetGoalsOfSegments(projectID, isOnlyRunningOnes, getPresentValues);

            return goals;
        }

        [HttpGet]
        [Route("api/GoalsOfSegments/{id}")]
        public string Get(int id)
        {
            return "value" + id;
        }


        [HttpGet]
        [Route("api/GoalsOfSegments/")]
        public string Get()
        {
            return "value";
        }

        [HttpPost]
        [Route("api/GoalsOfSegments/")]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut]
        [Route("api/GoalsOfSegments/{eventID}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete]
        [Route("api/GoalsOfSegments/{eventID}")]
        public void Delete(int id)
        {
        }
    }
}
