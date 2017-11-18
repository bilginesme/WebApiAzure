using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class GoalsController : ApiController
    {
        [HttpGet]
        [Route("api/Goals/{ownerID}")]
        public IEnumerable<GoalInfo> Get(long ownerID)
        {
            OwnerInfo owner = DB.Owner.GetOwner(DTC.RangeEnum.Week, DateTime.Today);
            List<GoalInfo> goals = DB.Goals.GetGoals(owner, true);

            goals = goals.OrderByDescending(i => i.PresentPercentage).ToList();

            return goals;
        }

        // GET: api/Goals/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Goals
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Goals/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Goals/5
        public void Delete(int id)
        {
        }
    }
}
