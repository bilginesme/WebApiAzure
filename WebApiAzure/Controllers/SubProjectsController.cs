using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class SubProjectsController : ApiController
    {
        [Route("api/SubProjects/{projectID}/{isOnlyRunning}")]
        public IEnumerable<SubProjectInfo> Get(int projectID, bool isOnlyRunning)
        {
            List<SubProjectInfo> subProjects = DB.SubProjects.GetSubProjects(projectID, isOnlyRunning);

            return subProjects;
        }

        [Route("api/SubProjects/{projectID}/{isOnlyRunning}/{isCalculatePerformance}")]
        public IEnumerable<SubProjectInfo> Get(int projectID, bool isOnlyRunning, bool isCalculatePerformance)
        {
            List<SubProjectInfo> subProjects = DB.SubProjects.GetSubProjects(projectID, isOnlyRunning, isCalculatePerformance);

            return subProjects;
        }

        [HttpGet]
        [Route("api/SubProjects/{subProjectID}")]
        public SubProjectInfo Get(long subProjectID)
        {
            return DB.SubProjects.GetSubProject(subProjectID);
        }
         
        [HttpPost]
        [Route("api/SubProjects/")]
        public void Post([FromBody] SubProjectInfo subProject)
        {
            DB.SubProjects.AddUpdateSubProject(subProject);
        }

        [HttpPut]
        [Route("api/SubProjects/{subProjectID}")]
        public void Put(long subProjectID, [FromBody] SubProjectInfo subProject)
        {
            DB.SubProjects.AddUpdateSubProject(subProject);
        }

        [HttpDelete]
        [Route("api/SubProjects/{subProjectID}")]
        public bool Delete(long subProjectID)
        {
            return DB.SubProjects.DeleteSubProject(subProjectID);
        }
    }
}
