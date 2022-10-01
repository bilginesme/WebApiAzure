using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class TasksController : ApiController
    {
        [HttpGet]
        [Route("api/Tasks/")]
        public IEnumerable<TaskInfo> Get()
        {
            return DB.Tasks.GetTasks(DateTime.Today, DB.TaskStatusEnum.All);
        }

        [HttpGet]
        [Route("api/Tasks/{parameter}/{strDate}")]
        public IEnumerable<TaskInfo> Get(int parameter, string strDate)
        {
            List<TaskInfo> tasks = new List<TaskInfo>();
            DateTime theDate = DateTime.Today;

            if (strDate != string.Empty)
                theDate = DTC.Date.GetDateFromString(strDate, DTC.Date.DateStyleEnum.Universal);

            if (parameter == 1)
                tasks = DB.Tasks.GetTasks(theDate, DB.TaskStatusEnum.Running);
            else if (parameter == 2)
                tasks = DB.Tasks.GetTasks(theDate, DB.TaskStatusEnum.All);

            return tasks;
        }

        [HttpGet]
        [Route("api/Tasks/{parameter1}/{parameter2}/{parameter3}")]
        public IEnumerable<TaskInfo> Get(int parameter1, string parameter2, string parameter3)
        {
            List<TaskInfo> tasks = new List<TaskInfo>();
            
            if(parameter1 == 1)
            {
                long blockID = Convert.ToInt32(parameter2);
                bool isOnlyRunning = Convert.ToBoolean(parameter3);

                DB.TaskStatusEnum taskStatus = DB.TaskStatusEnum.All;
                if (isOnlyRunning)
                    taskStatus = DB.TaskStatusEnum.Running;

                tasks = DB.Tasks.GetTasksOfBlock(blockID, taskStatus);
            }
            else if (parameter1 == 2)
            {
                DateTime dateStart = DTC.Date.GetDateFromString(parameter2, DTC.Date.DateStyleEnum.Universal);
                DateTime dateEnd = DTC.Date.GetDateFromString(parameter3, DTC.Date.DateStyleEnum.Universal);
                tasks = DB.Tasks.GetTasks(dateStart, dateEnd, DB.TaskStatusEnum.All);
            }

            return tasks;
        }

        [HttpGet]
        [Route("api/Tasks/{id}")]
        public TaskInfo Get(long id)
        {
            TaskInfo task = DB.Tasks.GetTask(id);

            if (task == null)
                task = new TaskInfo();

            return task;
        }

        [HttpPost]
        [Route("api/Tasks/")]
        public void Post([FromBody]TaskInfo task)
        {
            DB.Tasks.AddUpdateTask(task);
        }

        [HttpPut]
        [Route("api/Tasks/{taskID}")]
        public void Put(int taskID, [FromBody]TaskInfo task)
        {
            DB.Tasks.AddUpdateTask(task);
        }

        [HttpPut]
        [Route("api/Tasks/{taskID}/{order}")]
        public void Put(long taskID, int order, [FromBody]TaskInfo task)
        {
            DB.Tasks.UpdateTaskOrder(taskID, order);
        }

        [HttpDelete]
        [Route("api/Tasks/{taskID}")]
        public void Delete(long taskID)
        {
            DB.Tasks.DeleteTask(taskID);
        }
    }
}