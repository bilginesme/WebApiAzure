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
        // GET: api/ActionableProjects
        public IEnumerable<ProjectSnapshotInfo> Get()
        {
            List<ProjectSnapshotInfo> projectsSnapshot = DB.Projects.GetProjectsSnapshot();

            projectsSnapshot = projectsSnapshot.OrderByDescending(i=>i.RealTime).OrderBy(i => i.Rank).ToList();
            return projectsSnapshot;
        }

        // GET: api/ActionableProjects/5
        public ProjectSnapshotInfo Get(int id)
        {
            ProjectSnapshotInfo ps = DB.Projects.GetProjectSnapshot(id);
            return ps;
        }

        // POST: api/ActionableProjects
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/ActionableProjects/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ActionableProjects/5
        public void Delete(int id)
        {
        }
    }
}
