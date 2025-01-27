using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;
using static WebApiAzure.DB;

namespace WebApiAzure.Controllers
{
    public class GoalsOfSegmentsController : ApiController
    {
        [HttpGet]
        [Route("api/GoalsOfSegments/")]
        public IEnumerable<GoalInfo> Get()
        {
            List<GoalInfo> goals = DB.Goals.GetGoalsOfSegments();

            return goals;
        }

        [HttpGet]
        [Route("api/GoalsOfSegments/{projectID}/{isOnlyRunningOnes}/{getPresentValues}")]
        public IEnumerable<GoalInfo> Get(int projectID, bool isOnlyRunningOnes, bool getPresentValues)
        {
            List<GoalInfo> goals = DB.Goals.GetGoalsOfSegments(projectID, isOnlyRunningOnes, getPresentValues);

            return goals;
        }


        [HttpGet]
        [Route("api/GoalsOfSegments/{param1}/{param2}/{strStartDate}/{strEndDate}")]
        public IEnumerable<GoalInfo> Get(string param1, string param2, string strStartDate, string strEndDate)
        {
            DateTime dateStart = DTC.Date.GetDateFromString(strStartDate, DTC.Date.DateStyleEnum.Universal);
            DateTime dateEnd= DTC.Date.GetDateFromString(strEndDate, DTC.Date.DateStyleEnum.Universal);

            List<GoalInfo> goals = new List<GoalInfo>();

            if (param1 == "1") 
            {
                goals = DB.Goals.GetGoalsOfSegments(dateStart, dateEnd);
            }

            return goals;
        }

        [HttpGet]
        [Route("api/GoalsOfSegments/{id}")]
        public string Get(int id)
        {
            return "value" + id;
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
