using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class SegmentsController : ApiController
    {
        [Route("api/Blocks/{projectID}/{isOnlyRunning}")]
        public IEnumerable<SegmentInfo> Get(int blockID, bool isOnlyRunning)
        {
            List<SegmentInfo> segments = DB.GetSegments(blockID);

            return segments;
        }

        // GET: api/Blocks/5
        public string Get(int id)
        {
            return "value";
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
