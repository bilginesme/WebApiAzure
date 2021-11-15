using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class GoalTypesController : ApiController
    {
        // GET: api/GoalGroups
        public IEnumerable<GoalTypeInfo> Get()
        {
            return DB.Goals.GetGoalTypes();
        }

        // GET: api/GoalGroups/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/GoalGroups
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/GoalGroups/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/GoalGroups/5
        public void Delete(int id)
        {
        }
    }
}
