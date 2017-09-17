using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiAzure.Controllers
{
    public class OldestSegmentsController : ApiController
    {
        [Route("api/OldestSegments/{projectID}")]
        public IEnumerable<SegmentInfo> Get(long projectID)
        {
            return DB.GetOldestSegments(projectID);
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