using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ProjectGroupsController : ApiController
    {
        // GET: api/ProjectGroups
        [HttpGet]
        [Route("api/ProjectGroups/")]
        public IEnumerable<ProjectGroupInfo> Get()
        {
            List<ProjectGroupInfo> data = DB.ProjectGroups.GetProjectGroups();
            return data;
        }

        [HttpGet]
        [Route("api/ProjectGroups/{projectTypeID}/{statusID}")]
        public IEnumerable<ProjectGroupInfo> Get(int projectTypeID, int statusID)
        {
            List<ProjectGroupInfo> data = DB.ProjectGroups.GetProjectGroups();

            if(projectTypeID > 0)
                data = data.FindAll(i => i.ProjectTypeID == projectTypeID);

            if (statusID > 0)
                data = data.FindAll(i => i.Status == (DTC.StatusEnum)statusID);

            return data;
        }

        [HttpGet]
        [Route("api/ProjectGroups/{projectGroupID}")]
        public ProjectGroupInfo Get(int projectGroupID)
        {
            return DB.ProjectGroups.GetProjectGroup(projectGroupID);
        }

        [HttpPost]
        [Route("api/ProjectGroups/")]
        public void Post([FromBody]ProjectGroupInfo projectGroup)
        {
            DB.ProjectGroups.AddUpdateProjectGroup(projectGroup);
        }

        [HttpPut]
        [Route("api/ProjectGroups/{projectGroupID}")]
        public void Put(int projectGroupID, [FromBody]ProjectGroupInfo projectGroup)
        {
            DB.ProjectGroups.AddUpdateProjectGroup(projectGroup);
        }

        [HttpDelete]
        [Route("api/ProjectGroups/{projectGroupID}")]
        public string Delete(int projectGroupID)
        {
            return DB.ProjectGroups.DeleteProjectGroup(projectGroupID);
        }
    }
}
