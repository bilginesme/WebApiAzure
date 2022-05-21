using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class IdeasController : ApiController
    {
        public IEnumerable<IdeaInfo> Get()
        {
            return new List<IdeaInfo>();
        }

        [HttpGet]
        [Route("api/Ideas/{parameter}/{ideaGroupID}/")]
        public IEnumerable<IdeaInfo> Get(int parameter, int ideaGroupID)
        {
            List<IdeaInfo> data = new List<IdeaInfo>();

            if (parameter == 1)
                data = DB.Ideas.GetIdeas(ideaGroupID);

            return data;
        }

        [HttpGet]
        [Route("api/Ideas/{ideaID}")]
        public IdeaInfo Get(int ideaID)
        {
            return DB.Ideas.GetIdea(ideaID);
        }

        [HttpPost]
        [Route("api/Ideas/")]
        public void Post([FromBody] IdeaInfo idea)
        {
            DB.Ideas.AddUpdateIdea(idea);
        }

        [HttpPut]
        [Route("api/Ideas/{ideaGroupID}")]
        public void Put(long ideaID, [FromBody] IdeaInfo idea)
        {
            idea.ID = ideaID;
            DB.Ideas.AddUpdateIdea(idea);
        }

        [HttpDelete]
        [Route("api/Ideas/{ideaGroupID}")]
        public void Delete(long ideaGroupID)
        {
            DB.IdeaGroups.DeleteIdeaGroup(ideaGroupID);
        }
    }
}
