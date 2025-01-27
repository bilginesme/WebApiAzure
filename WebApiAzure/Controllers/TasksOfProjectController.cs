using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class TasksOfProjectController : ApiController
    {
        [HttpGet]
        [Route("api/TasksOfProject/{projectID}/{numTasks}")]
        public IEnumerable<TaskInfo> Get(long projectID, int numTasks)
        {
            return DB.Tasks.GetTasks(projectID, DB.TaskStatusEnum.Completed, numTasks);
        }

        [HttpGet]
        [Route("api/TasksOfProject/{projectID}/{strDateStart}/{strDateEnd}")]
        public IEnumerable<TaskInfo> Get(int parameter, string strDateStart, string strDateEnd)
        {
            List<TaskInfo> tasks = new List<TaskInfo>();
            DateTime theDate = DateTime.Today;

       

            return tasks;
        }

   
    }
}