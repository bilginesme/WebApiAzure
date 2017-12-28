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
        [Route("api/Segments/{blockID}/{isOnlyRunning}")]
        public IEnumerable<SegmentInfo> Get(long blockID, string isOnlyRunning)
        {
            List<SegmentInfo> segments = DB.GetSegments(blockID);

            return segments;
        }

        [HttpGet]
        [Route("api/Segments/{segmentID}")]
        public SegmentInfo Get(long segmentID)
        {
            return DB.GetSegment(segmentID);
        }
         
        [HttpPost]
        [Route("api/Segments/")]
        public void Post([FromBody] SegmentInfo segment)
        {
            DB.AddSegment(segment);
        }

        [HttpPut]
        [Route("api/Segments/{segmentID}")]
        public void Put(long segmentID, [FromBody] SegmentInfo segment)
        {
            DB.UpdateSegment(segment);
        }

        [HttpDelete]
        [Route("api/Segments/{segmentID}")]
        public string Delete(long segmentID)
        {
            DB.DeleteSegment(segmentID);
            return "ok";
        }
    }
}
