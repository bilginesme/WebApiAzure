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

        [Route("api/Segments/{blockID}/{strTitle}/{strDetails}")]
        public string Get(long blockID, string strTitle, string strDetails)
        {
            return DB.AddSegment(blockID, strTitle, strDetails);
        }

        [Route("api/Segments/{segmentID}/{strTitle}/{statusID}/{strDetails}")]
        public string Get(long segmentID, string strTitle, int statusID, string strDetails)
        {
            SegmentInfo segment = DB.GetSegment(segmentID);

            segment.Status = (DTC.StatusEnum)statusID;
            segment.Title = strTitle;
            segment.Details = strDetails;
            return DB.UpdateSegment(segment);
        }


        [HttpGet]
        [Route("api/Segments/{segmentID}")]
        public SegmentInfo Get(long segmentID)
        {
            return DB.GetSegment(segmentID);
        }

        public void Post([FromBody]int segmentID)
        {
            //DB.SaveSegment(segmentID, title);
        }

        // PUT: api/Blocks/5
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete]
        [Route("api/Segments/{segmentID}")]
        public string Delete(int segmentID)
        {
            DB.DeleteSegment(segmentID);
            return "ok";
        }
    }
}
