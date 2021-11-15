using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class OwnerController : ApiController
    {
        // GET: api/GoalGroups
        public IEnumerable<OwnerInfo> Get()
        {
            return new List<OwnerInfo>();
        }


        // GET: api/GoalGroups/5
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        [Route("api/Owner/{rangeID}/{strDate}")]
        public OwnerInfo Get(int rangeID, string strdate)
        {
            DateTime theDate = DTC.Date.GetDateFromString(strdate, DTC.Date.DateStyleEnum.Universal);
            OwnerInfo owner = new OwnerInfo();

            owner = DB.Owner.GetOwner((DTC.RangeEnum)rangeID, theDate);
            return owner;
        }

        [HttpGet]
        [Route("api/Owner/{prevOrNext}/{rangeID}/{strDate}")]
        public OwnerInfo Get(int prevOrNext, int rangeID, string strdate)
        {
            DateTime theDate = DTC.Date.GetDateFromString(strdate, DTC.Date.DateStyleEnum.Universal);
            OwnerInfo owner = new OwnerInfo();

            owner = DB.Owner.GetPrevNextOwner(prevOrNext, (DTC.RangeEnum)rangeID, theDate);

            return owner;
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
