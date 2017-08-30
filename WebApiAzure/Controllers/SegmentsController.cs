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

        [Route("api/Segments/{id}/{purpose}/{strTitle}")]
        public string Get(long id, string purpose, string strTitle)
        {
            if (purpose == "add")
                return DB.AddSegment(id, strTitle);
            else
                return "NA";
        }

        [Route("api/Segments/{id}/{purpose}/{strTitle}/{strStatusID}")]
        public string Get(long id, string purpose, string strTitle, string strStatusID)
        {
            if(DTC.IsNumeric(strStatusID))
            {
                DTC.StatusEnum status = (DTC.StatusEnum)Convert.ToInt16(strStatusID);
                return DB.UpdateSegment(id, strTitle, status);
            }
            else
            {
                return "error";
            }
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
