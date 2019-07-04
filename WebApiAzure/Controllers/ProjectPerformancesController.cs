using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ProjectPerformancesController : ApiController
    {
        [HttpGet]
        [Route("api/ProjectPerformances/")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("api/ProjectPerformances/{projectID}/{nTop}")]
        public IEnumerable<WeeklyPerformanceInfo> Get(int projectID, int ntop)
        {
            List<WeeklyPerformanceInfo> data = new List<WeeklyPerformanceInfo>();

            List<Tuple<int, int, float>> dataRaw = DB.Projects.GetWeeklyAverageLogs(projectID, ntop);

            for(int i=0;i<dataRaw.Count;i++)
            {
                string strLabel = "W " + dataRaw[i].Item2;
                float percentage = 100 * dataRaw[i].Item3;

                WeeklyPerformanceInfo item = new WeeklyPerformanceInfo();
                item.Label = strLabel;
                item.Percentage = percentage;

                data.Add(item);
            }

            return data;
        }

        // POST: api/ProjectPerformances
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/ProjectPerformances/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ProjectPerformances/5
        public void Delete(int id)
        {
        }
    }
}
