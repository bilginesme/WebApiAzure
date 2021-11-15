using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class GoalsOfFocusController : ApiController
    {
        [HttpGet]
        [Route("api/GoalsOfFocus/{rangeID}/{strDateStart}/{strDateEnd}/{isGetPresentValues}")]
        public IEnumerable<GoalInfo> Get(int rangeID, string strDateStart, string strDateEnd, bool isGetPresentValues)
        {
            DateTime dtStart = DateTime.Today;
            DateTime dtEnd = DateTime.Today;
            DTC.RangeEnum range = (DTC.RangeEnum)rangeID;
            if (strDateStart != string.Empty)
                dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);

            if (strDateEnd != string.Empty)
                dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            List<GoalInfo> goals = DB.Goals.GetImportantGoals(range, dtStart, dtEnd, isGetPresentValues).FindAll(i=>i.IsFocus);

            DayInfo day = DB.Days.GetDay(DateTime.Today, true);

            foreach (GoalInfo goal in goals)
            {
                goal.PresentPercentage = goal.GetPerformance(false, day);
                goal.DesiredValue = goal.GetDesiredValue(day);
            }

            goals = goals.OrderByDescending(i => i.PresentPercentage).ToList();

            return goals;
        }

        // GET: api/GoalsOfProject/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/GoalsOfProject
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/GoalsOfProject/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/GoalsOfProject/5
        public void Delete(int id)
        {
        }
    }
}
