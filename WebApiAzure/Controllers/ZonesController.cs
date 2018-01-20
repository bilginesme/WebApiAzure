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

        [HttpGet]
        [Route("api/Zones/{zoneID}")]
        public ZoneInfo Get(long zoneID)
        {
            return DB.Zones.GetZone(zoneID);
        }
         
        [HttpPost]
        [Route("api/Zones/")]
        public void Post([FromBody] ZoneInfo zone)
        {
            DB.Zones.AddUpdateZone(zone);
        }

        [HttpPut]
        [Route("api/Zones/{zoneID}")]
        public void Put(long zoneID, [FromBody] ZoneInfo zone)
        {
            DB.Zones.AddUpdateZone(zone);
        }

        [HttpDelete]
        [Route("api/Zones/{zoneID}")]
        public string Delete(long zoneID)
        {
            DB.Zones.DeleteZone(zoneID);
            return "ok";
        }
    }
}
