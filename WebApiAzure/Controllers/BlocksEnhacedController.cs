using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class BlocksEnhacedController : ApiController
    {
        [Route("api/BlocksEnhanced/{projectID}/{isOnlyRunning}")]
        public IEnumerable<BlockEnhancedInfo> Get(int projectID, bool isOnlyRunning)
        {
            List<BlockEnhancedInfo> blocks = DB.GetBlocksEnhanced(projectID);

            return blocks;
        }

        [Route("api/BlocksEnhanced/{dummy}/{zoneID}/{projectID}")]
        public IEnumerable<BlockEnhancedInfo> Get(string dummy, long zoneID, int projectID)
        {
            // WARNING : This is extremely inefficient. Find another way of getting it
            ZoneInfo zone = DB.Zones.GetZone(zoneID);
            List<BlockEnhancedInfo> blocks = new List<BlockEnhancedInfo>();
            blocks = DB.GetBlocksEnhanced(projectID).FindAll(i=>i.ZoneID == zoneID);

            return blocks;
        }

        // POST: api/Blocks
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Blocks/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Blocks/5
        public void Delete(int id)
        {
        }
    }
}
