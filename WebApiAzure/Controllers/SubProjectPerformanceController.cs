using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class SubProjectPerformanceController : ApiController
    {
        [Route("api/SubProjectPerformance/{subProjectID}")]
        public SubProjectPerformanceInfo Get(long subProjectID)
        {
            SubProjectPerformanceInfo data = DB.SubProjects.GetSubProjectPerformance(subProjectID);

            return data;
        }
    }
}
