using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class RealHoursPerWeekController : ApiController
    {
        // GET: api/RealHoursPerWeek
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [Route("api/RealHoursPerWeek/{projectID}/{nTop}")]
        public IEnumerable<RealHoursPerWeekInfo> Get(int projectID, int nTop)
        {
            List<RealHoursPerWeekInfo> data = new List<RealHoursPerWeekInfo>();
            Dictionary<string, int> realMinutes = DB.GetRealMinutesPerWeek(projectID, nTop);
            ProjectInfo project = DB.GetProject(projectID);

            bool isOK = false;
            WeekInfo w = new WeekInfo(DateTime.Today);
            int count = 0;
            while(!isOK)
            {
                string keyCandidate = w.StartDate.Year.ToString() + w.WeekNO.ToString();

                if (realMinutes.ContainsKey(keyCandidate))
                {
                    decimal realHours = Convert.ToDecimal(DTC.Format2(Convert.ToDecimal(realMinutes[keyCandidate]) / 60));

                    data.Add(new RealHoursPerWeekInfo(w.StartDate.Year, w.WeekNO, realHours));
                }
                else
                {
                    data.Add(new RealHoursPerWeekInfo(w.StartDate.Year, w.WeekNO, 0));
                }

                w = DTC.GetPreviousWeek(w);
                count++;
                if (count > 20 || w.StartDate < project.StartDate)
                    isOK = true;
            }

            data.Reverse();

            /*
            foreach(string key in realMinutes.Keys)
            {
                int year = Convert.ToInt16(key.Substring(0, 4));
                int week = Convert.ToInt16(key.Substring(4, key.Length - 4));
                decimal realHours = Convert.ToDecimal(DTC.Format2(Convert.ToDecimal(realMinutes[key]) / 60));

                data.Add(new RealHoursPerWeekInfo(year, week, realHours));
            }
            */

            return data;
        }

        // POST: api/RealHoursPerWeek
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/RealHoursPerWeek/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/RealHoursPerWeek/5
        public void Delete(int id)
        {
        }
    }
}
