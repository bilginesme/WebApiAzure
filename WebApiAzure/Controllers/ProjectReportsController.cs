using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ProjectReportsController : ApiController
    {
        [HttpGet]
        [Route("api/ProjectReports/")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("api/ProjectReports/{projectGroupID}/{projectID}/{reportType}")]
        public IEnumerable<ReportItemInfo> Get(long projectGroupID, long projectID, int reportType)
        {
            List<ReportItemInfo> data = new List<ReportItemInfo>();

            if(reportType == 1)
            {
                Dictionary<int, float> dict = DB.Projects.GetYearlyHoursOfProject(projectID);

                foreach(int year in dict.Keys)
                {
                    ReportItemInfo repItem = new ReportItemInfo();
                    repItem.ID = year;
                    repItem.Label = year.ToString();
                    repItem.Hours = dict[year];
                    data.Add(repItem);
                }
            }

            return data;
        }

        // POST: api/ProjectPerformances
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/ProjectPerformances/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ProjectPerformances/5
        public void Delete(int id)
        {
        }
    }
}
