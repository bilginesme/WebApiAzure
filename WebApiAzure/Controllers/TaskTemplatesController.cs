using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class TaskTemplatesController : ApiController
    {
        [HttpGet]
        [Route("api/TaskTemplates/")]
        public IEnumerable<TaskTemplateInfo> Get()
        {
            List<TaskTemplateInfo> data = DB.TaskTemplates.GetTaskTemplates();
            return data;
        }

        [HttpGet]
        [Route("api/TaskTemplates/{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        [Route("api/TaskTemplates/")]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut]
        [Route("api/TaskTemplates/{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete]
        [Route("api/TaskTemplates/{id}")]
        public void Delete(int id)
        {
        }
    }
}