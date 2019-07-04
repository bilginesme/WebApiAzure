using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ProjectsController : ApiController
    {
        [HttpGet]
        [Route("api/Projects/")]
        public IEnumerable<ProjectInfo> Get()
        {
            DateTime dateEst = DB.Projects.GetEstimatedCompletionDate(383);

            List<ProjectInfo> data = DB.Projects.GetProjects();
            return data;
        }

        [HttpGet]
        [Route("api/Projects/{projectGroupID}/{statusID}")]
        public IEnumerable<ProjectInfo> Get(int projectGroupID, int statusID)
        {
            List<ProjectInfo> data = new List<ProjectInfo>();

            data = DB.Projects.GetProjects(projectGroupID, (DTC.StatusEnum)statusID);
            
            return data;
        }

        [HttpGet]
        [Route("api/Projects/{projectID}")]
        public ProjectInfo Get(int projectID)
        {
            return DB.Projects.GetProject(projectID);
        }

        [HttpGet]
        [Route("api/Projects/{projectID}/{isUpdateCompletionRate}/{reservedAction}")]
        public ProjectInfo Get(int projectID, bool isUpdateCompletionRate, int reservedAction)
        {
            if (isUpdateCompletionRate)
                DB.Projects.UpdateCompletionRateAndHoursNeeded(projectID);

            return DB.Projects.GetProject(projectID);
        }

        [HttpGet]
        [Route("api/Projects/{projectGroupID}/{statusID}/{strDateStart}/{strDateEnd}/{actionID}")]
        public IEnumerable<ProjectInfo> Get(int projectGroupID, int statusID, string strDateStart, string strDateEnd, int actionID)
        {
            List<ProjectInfo> data = new List<ProjectInfo>();

            DateTime dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);
            DateTime dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            if (actionID == 1)
                data = DB.Projects.GetProjectsRelatedToTasks(dtStart);

            return data;
        }

        [HttpPost]
        [Route("api/Projects/")]
        public void Post([FromBody]ProjectInfo project)
        {
            DB.Projects.AddUpdateProject(project);
        }

        [HttpPut]
        [Route("api/Projects/{projectID}")]
        public void Put(int projectID, [FromBody]ProjectInfo project)
        {
            DB.Projects.AddUpdateProject(project);
        }

        [HttpDelete]
        [Route("api/Projects/{projectID}")]
        public void Delete(int projectID)
        {
            
        }
    }
}
