using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ProjectGroupReportsController : ApiController
    {
        [HttpGet]
        [Route("api/ProjectGroupReports/")]
        public IEnumerable<ProjectGroupInfo> Get()
        {
            List<ProjectGroupInfo> data = DB.ProjectGroups.GetProjectGroups();
            return data;
        }

        [HttpGet]
        [Route("api/ProjectGroupReports/{numItems}/{strDateStart}/{strDateEnd}")]
        public IEnumerable<ProjectGroupReportInfo> Get(int numItems, string strDateStart, string strDateEnd)
        {
            List<ProjectGroupReportInfo> data = new List<ProjectGroupReportInfo>();
            DateTime dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);
            DateTime dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            data = DB.ProjectGroups.GetProjectGroupReport(numItems, dtStart, dtEnd);

            return data;
        }

        [HttpPost]
        [Route("api/ProjectGroupReports")]
        public void Post([FromBody]ProjectGroupInfo projectGroup)
        {
            DB.ProjectGroups.AddUpdateProjectGroup(projectGroup);
        }

        [HttpPut]
        [Route("api/ProjectGroupReports/{projectGroupID}")]
        public void Put(int projectGroupID, [FromBody]ProjectGroupInfo projectGroup)
        {
            DB.ProjectGroups.AddUpdateProjectGroup(projectGroup);
        }

        [HttpDelete]
        [Route("api/ProjectGroupReports/{projectGroupID}")]
        public string Delete(int projectGroupID)
        {
            return DB.ProjectGroups.DeleteProjectGroup(projectGroupID);
        }
    }
}
