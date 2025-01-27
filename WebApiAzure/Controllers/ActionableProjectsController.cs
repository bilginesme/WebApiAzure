using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ActionableProjectsController : ApiController
    {
        [HttpGet]
        [Route("api/ActionableProjects/")]
        public IEnumerable<ProjectSnapshotInfo> Get()
        {
            //DateTime dt = new DateTime(2019, 6, 15);
            //int k = (int) dt.Subtract(DateTime.Now).TotalDays;

            List<ProjectSnapshotInfo> projectsSnapshot = DB.Projects.GetProjectsSnapshot();

            projectsSnapshot = projectsSnapshot.OrderByDescending(i=>i.RealTime).OrderBy(i => i.Rank).ToList();
            return projectsSnapshot;
        }

        [HttpGet]
        [Route("api/ActionableProjects/{isUpdateCompletionRate}/{rankID}")]
        public IEnumerable<ProjectSnapshotInfo> Get(bool isUpdateCompletionRate, int rankID)
        {
            List<ProjectSnapshotInfo> projectsSnapshot = DB.Projects.GetProjectsSnapshot();

            if (isUpdateCompletionRate)
            {
                foreach (ProjectSnapshotInfo ps in projectsSnapshot)
                    DB.Projects.UpdateCompletionRateAndHoursNeeded(ps.ProjectID);
            }

            if (rankID > 0)
                projectsSnapshot = projectsSnapshot.FindAll(i => i.Rank == (DTC.RankEnum)rankID);

            //projectsSnapshot = projectsSnapshot.OrderByDescending(i => i.RealTime).OrderBy(i => i.Rank).ToList();
            
            return projectsSnapshot;
        }

        [HttpGet]
        [Route("api/ActionableProjects/{projectID}")]
        public ProjectSnapshotInfo Get(int projectID)
        {
            ProjectSnapshotInfo ps = DB.Projects.GetProjectSnapshot(projectID);
            return ps;
        }

        [HttpPost]
        [Route("api/ActionableProjects/")]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut]
        [Route("api/ActionableProjects/{projectID}")]
        public void Put(int projectID, [FromBody]string value)
        {
        }

        [HttpDelete]
        [Route("api/ActionableProjects/{projectID}")]
        public void Delete(int projectID)
        {
        }
    }
}
