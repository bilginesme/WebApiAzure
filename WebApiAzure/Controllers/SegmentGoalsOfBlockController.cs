using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class SegmentGoalsOfBlockController : ApiController
    {

        [HttpGet]
        [Route("api/SegmentGoalsOfBlock/{blockID}/{isOnlyRunningOnes}/{getPresentValues}")]
        public IEnumerable<GoalInfo> Get(int blockID, bool isOnlyRunningOnes, bool getPresentValues)
        {
            List<GoalInfo> goals = DB.Goals.GetSegmentGoalsOfBlock(blockID, isOnlyRunningOnes, getPresentValues);

            return goals;
        }

        [HttpGet]
        [Route("api/SegmentGoalsOfBlock/{id}")]
        public string Get(int id)
        {
            return "value" + id;
        }


        [HttpGet]
        [Route("api/SegmentGoalsOfBlock/")]
        public string Get()
        {
            return "value";
        }

        [HttpPost]
        [Route("api/SegmentGoalsOfBlock/")]
        public void Post([FromBody]string value)
        {
        }

        [HttpPost]
        [Route("api/SegmentGoalsOfBlock/{segmentID}/{strDate}")]
        public long Post(long segmentID, string strDate)
        {
            DateTime theDateOfTheDay = DTC.Date.GetDateFromString(strDate, DTC.Date.DateStyleEnum.Universal);
            return DB.Goals.AddSegmentGoal(segmentID, theDateOfTheDay);
        }

        [HttpPut]
        [Route("api/SegmentGoalsOfBlock/{id}")]
        public void Put(int id)
        {
            DB.Goals.CompleteSegmentGoal(id);
        }

        [HttpDelete]
        [Route("api/SegmentGoalsOfBlock/{id}")]
        public void Delete(int id)
        {
        }
    }
}
