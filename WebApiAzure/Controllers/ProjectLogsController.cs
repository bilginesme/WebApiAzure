using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ProjectLogsController : ApiController
    {
        [HttpGet]
        [Route("api/ProjectLogs/")]
        public IEnumerable<ProjectLogInfo> Get()
        {
            return null;
        }

        [HttpGet]
        [Route("api/ProjectLogs/{projectID}/{lastN}")]
        public IEnumerable<ProjectLogInfo> Get(long projectID, int lastN)
        {
            return DB.ProjectLogs.GetProjectLogsLastN(projectID, lastN);
        }

        [HttpPost]
        [Route("api/ProjectLogs/")]
        public void Post([FromBody] ProjectLogInfo value)
        {
        }

        [HttpPut]
        [Route("api/ProjectLogs/{projectLogID}")]
        public void Put(long projectLogID, [FromBody] ProjectLogInfo value)
        {
        }

        [HttpDelete]
        [Route("api/ProjectLogs/{projectLogID}")]
        public void Delete(long projectLogID)
        {
        }
    }
}
