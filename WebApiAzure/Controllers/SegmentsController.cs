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

        [Route("api/Segments/{projectID}/{blockID}/{isOnlyRunning}")]
        public IEnumerable<SegmentInfo> Get(int projectID, long blockID, bool isOnlyRunning)
        {
            List<SegmentInfo> segments = DB.Segments.GetSegments(projectID);

            if (isOnlyRunning)
                segments = segments.FindAll(q => q.Status == DTC.StatusEnum.Running);

            return segments;
        }

        [Route("api/Segments/{projectID}/{param1}/{param2}/{param3}")]
        public IEnumerable<SegmentInfo> Get(int projectID, string param1, string param2, string param3)
        {
            List<SegmentInfo> segments = new List<SegmentInfo>();

            if(param1 == "LATEST")
            {
                int numSegments = Convert.ToInt16(param2);
                
                segments = DB.Segments.GetSegmentsCompleted(projectID, numSegments);
            }
            else if (param1 == "SEGMENTS_COMPLETED")
            {
                DateTime dateStart = DTC.Date.GetDateFromString(param2, DTC.Date.DateStyleEnum.Universal);
                DateTime dateEnd = DTC.Date.GetDateFromString(param3, DTC.Date.DateStyleEnum.Universal);

                segments = DB.Segments.GetSegmentsCompleted(dateStart, dateEnd, 0);
            }

            return segments;
        }

        [HttpGet]
        [Route("api/Segments/{segmentID}")]
        public SegmentInfo Get(long segmentID)
        {
            return DB.Segments.GetSegment(segmentID);
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
