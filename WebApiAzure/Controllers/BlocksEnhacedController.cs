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

        [Route("api/BlocksEnhanced/{dummy}/{clusterID}/{projectID}")]
        public IEnumerable<BlockEnhancedInfo> Get(string dummy, long clusterID, int projectID)
        {
            // WARNING : This is extremely inefficient. Find another way of getting it
            ClusterInfo cluster = DB.Clusters.GetCluster(clusterID);
            List<BlockEnhancedInfo> blocks = new List<BlockEnhancedInfo>();
            blocks = DB.GetBlocksEnhanced(projectID).FindAll(i=>i.ClusterID == clusterID);

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
