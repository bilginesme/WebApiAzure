using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ZonesController : ApiController
    {
        [Route("api/Zones/{projectID}/{isOnlyRunning}")]
        public IEnumerable<ZoneInfo> Get(int projectID, bool isOnlyRunning)
        {
            List<ZoneInfo> zones = DB.Zones.GetZones(projectID, true);

            return zones;
        }

        public BlockInfo Get(long id)
        {
            return DB.GetBlock(id);
        }

        public void Post([FromBody]string value)
        {
        }

        public void Put(int id, [FromBody]string value)
        {
        }

        public void Delete(int id)
        {
        }
    }
}
