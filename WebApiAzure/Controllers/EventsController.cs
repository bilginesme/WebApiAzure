using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class EventsController : ApiController
    {
        [HttpGet]
        [Route("api/Events/")]
        public IEnumerable<DayInfo> Get()
        {
            return new List<DayInfo>();
        }

        [HttpGet]
        [Route("api/Events/{strDateStart}/{strDateEnd}")]
        public IEnumerable<NewsInfo> Get(string strDateStart, string strDateEnd)
        {
            DateTime dtStart = DateTime.Today;
            DateTime dtEnd = DateTime.Today;

            List<NewsInfo> events = new List<NewsInfo>();

            if (strDateStart != string.Empty)
                dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);

            if (strDateEnd != string.Empty)
                dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            events = DB.News.GetNews(dtStart, dtEnd);

            return events;
        }

        [HttpGet]
        [Route("api/Events/{param1}/{param2}/{param3}")]
        public IEnumerable<NewsInfo> Get(string param1, string param2, string param3)
        {
            List<NewsInfo> events = new List<NewsInfo>();
 
            if (param1 == "1")
            {
                DateTime theDate = DTC.Date.GetDateFromString(param2, DTC.Date.DateStyleEnum.Universal);
                events = DB.News.GetTheDayInHistory(theDate);
            }

            return events;
        }


        [HttpGet]
        [Route("api/Events/{range}/{parameter}/{strDateStart}/{strDateEnd}")]
        public IEnumerable<NewsInfo> Get(DTC.RangeEnum range, int parameter, string strDateStart, string strDateEnd)
        {
            DateTime dtStart = DateTime.Today;
            DateTime dtEnd = DateTime.Today;

            List<NewsInfo> events = new List<NewsInfo>();

            if (strDateStart != string.Empty)
                dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);

            if (strDateEnd != string.Empty)
                dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            if(parameter == 1)
            {
                if (range == DTC.RangeEnum.Day)
                {
                    events = DB.News.GetNews(dtStart, dtEnd);
                }
                else if (range == DTC.RangeEnum.Week)
                {
                    WeekInfo week = DB.Weeks.GetWeek(dtStart, false);
                    events = DB.News.GetNews(week.StartDate, week.EndDate);
                    events = events.OrderBy(i => i.Date).ToList();
                }
                    
            }
            else if (parameter == 2)
            {
                if (range == DTC.RangeEnum.Day)
                {
                    events = DB.News.GetTheDayInHistory(dtStart);
                }
                else if (range == DTC.RangeEnum.Week)
                {
                    WeekInfo week = DB.Weeks.GetWeek(dtStart, false);
                    events = DB.News.GetNewsLastYear(week.StartDate, week.EndDate);
                }
            }

            return events;
        }

        [HttpGet]
        [Route("api/Events/{eventID}")]
        public NewsInfo Get(int eventID)
        {
            return DB.News.GetNews(eventID);
        }
      
        [HttpPost]
        [Route("api/Events/")]
        public void Post([FromBody]NewsInfo value)
        {
        }

        [HttpPut]
        [Route("api/Events/{eventID}")]
        public void Put(int eventID, [FromBody]DayInfo value)
        {
        }

        [HttpDelete]
        [Route("api/Events/{eventID}")]
        public void Delete(int eventID)
        {
        }
    }
}
