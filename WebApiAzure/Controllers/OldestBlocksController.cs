using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiAzure.Controllers
{
    public class OldestBlocksController : ApiController
    {
        [Route("api/OldestBlocks/{projectID}")]
        public IEnumerable<BlockInfo> Get(long projectID)
        {
            return DB.GetOldestBlocks(projectID);
        }

        // POST: api/OldestSegments
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/OldestSegments/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/OldestSegments/5
        public void Delete(int id)
        {
        }
    }
}