using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class IdeaGroupsController : ApiController
    {
        public IEnumerable<IdeaGroupInfo> Get()
        {
            /*
            List<ProjectInfo> projects = DB.Projects.GetProjects();
            List<ProjectGroupInfo> projectGroups = DB.ProjectGroups.GetProjectGroups();

            foreach (IdeaGroupInfo iG in DB.IdeaGroups.GetIdeaGroups())
            {
                if(iG.ProjectID > 0)
                {
                    ProjectInfo project = projects.Find(i=>i.ID == iG.ProjectID);
                    ProjectGroupInfo projectGroup = projectGroups.Find(i => i.ID == iG.ProjectGroupID);

                    if (project != null)
                    {
                        string strProjectLabelLong = project.GetSmartCode(projectGroup, false);
                        string strProjectLabelShort = project.GetSmartCode(projectGroup, true);

                        iG.ProjectLabelLong = strProjectLabelLong;
                        iG.ProjectLabelShort = strProjectLabelShort;

                        DB.IdeaGroups.AddUpdateIdeaGroup(iG);
                    }
                }
            }
            */

            return DB.IdeaGroups.GetIdeaGroups();
        }

        [HttpGet]
        [Route("api/IdeaGroups/{isFocusedOnesOnTop}/{numIdeaGroups}")]
        public IEnumerable<IdeaGroupInfo> Get(bool isFocusedOnesOnTop, int numIdeaGroups)
        {
         
            return DB.IdeaGroups.GetIdeaGroups(isFocusedOnesOnTop, numIdeaGroups); 
        }

        [HttpGet]
        [Route("api/IdeaGroups/{ideaGroupID}")]
        public IdeaGroupInfo Get(int ideaGroupID)
        {
            return DB.IdeaGroups.GetIdeaGroup(ideaGroupID);
        }

        [HttpPost]
        [Route("api/IdeaGroups/")]
        public void Post([FromBody]IdeaGroupInfo ideaGroup)
        {
            DB.IdeaGroups.AddUpdateIdeaGroup(ideaGroup);
        }

        [HttpPut]
        [Route("api/IdeaGroups/{ideaGroupID}")]
        public void Put(long ideaGroupID, [FromBody]IdeaGroupInfo ideaGroup)
        {
            ideaGroup.ID = ideaGroupID;
            DB.IdeaGroups.AddUpdateIdeaGroup(ideaGroup);
        }

        [HttpDelete]
        [Route("api/IdeaGroups/{ideaGroupID}")]
        public void Delete(long ideaGroupID)
        {
            DB.IdeaGroups.DeleteIdeaGroup(ideaGroupID);
        }
    }
}
