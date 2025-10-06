using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class CoTasksController : ApiController
    {
        [HttpGet]
        [Route("api/CoTasks/")]
        public IEnumerable<CoTaskInfo> Get()
        {
            return null;
        }

        [HttpGet]
        [Route("api/CoTasks/{parameter}/{taskID}")]
        public IEnumerable<CoTaskInfo> Get(string parameter, long taskID)
        {
            List<CoTaskInfo> coTasks = new List<CoTaskInfo>();
            int coTasksFomat = Convert.ToInt16(parameter);

            if(coTasksFomat == 1)
            {
                coTasks = DB.CoTasks.GetCoTasks(taskID);
            }

            return coTasks;
        }

        [HttpGet]
        [Route("api/CoTasks/{parameter1}/{parameter2}/{parameter3}")]
        public IEnumerable<CoTaskInfo> Get(int parameter1, string parameter2, string parameter3)
        {
            List<CoTaskInfo> coTasks = new List<CoTaskInfo>();

            if (parameter1 == 1)
            {
                string strTaskIDs = parameter2;
                coTasks = DB.CoTasks.GetCoTasksOfTasks(strTaskIDs);
            }

            return coTasks;
        }

        [HttpGet]
        [Route("api/CoTasks/{coTaskID}")]
        public CoTaskInfo Get(long coTaskID)
        {
            return DB.CoTasks.GetCoTask(coTaskID);
        }

        [HttpPost]
        [Route("api/CoTasks/")]
        public void Post([FromBody] CoTaskInfo coTask)
        {
            DB.CoTasks.AddUpdateCoTask(coTask);
        }

        [HttpPut]
        [Route("api/CoTasks/{parameter1}/{parameter2}")]
        public IEnumerable<CoTaskInfo> Put(int parameter1, string parameter2, [FromBody] GenericItemInfo strTaskIDs)
        {
            List<CoTaskInfo> coTasks = new List<CoTaskInfo>();

            if (parameter1 == 1)
            {
                if(strTaskIDs.Name != "")
                { coTasks = DB.CoTasks.GetCoTasksOfTasks(strTaskIDs.Name); 
                }
            }

            return coTasks;
        }

        [HttpPut]
        [Route("api/CoTasks/{coTaskID}")]
        public void Put(long coTaskID, [FromBody] CoTaskInfo coTask)
        {
            DB.CoTasks.AddUpdateCoTask(coTask);
        }

        [HttpDelete]
        [Route("api/CoTasks/{coTaskID}")]
        public void Delete(long coTaskID)
        {
            DB.CoTasks.DeleteCoTask(coTaskID);
        }
    }
}
