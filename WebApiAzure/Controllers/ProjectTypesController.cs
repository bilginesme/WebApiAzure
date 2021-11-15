using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ProjectTypesController : ApiController
    {
        [HttpGet]
        [Route("api/ProjectTypes/")]
        public IEnumerable<ProjectTypeInfo> Get()
        {
            List<ProjectTypeInfo> data = DB.ProjectTypes.GetProjectTypes();
            return data;
        }
 
    }
}
