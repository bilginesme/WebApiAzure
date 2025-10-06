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
            else if (param1 == "2")
            {
                DateTime theDate = DTC.Date.GetDateFromString(param2, DTC.Date.DateStyleEnum.Universal);
                theDate = theDate.AddYears(-1);
                WeekInfo week = DB.Weeks.GetWeek(theDate, false);
                events = DB.News.GetNews(week.StartDate, week.EndDate);
            }
            else if (param1 == "3")
            {
                DateTime theDate = DTC.Date.GetDateFromString(param2, DTC.Date.DateStyleEnum.Universal);
                theDate = new DateTime(theDate.Year - 1, theDate.Month, 1);
                MonthInfo month = DB.Months.GetMonth(theDate, false);
                events = DB.News.GetNews(month.StartDate, month.EndDate);
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
        public long Post([FromBody]NewsInfo value)
        {
            value.Date = TimeZoneInfo.ConvertTimeFromUtc(value.Date, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));
            return DB.News.AddUpdateNews(value);
        }

        [HttpPut]
        [Route("api/Events/{eventID}")]
        public long Put(int eventID, [FromBody]NewsInfo value)
        {
            value.Date = TimeZoneInfo.ConvertTimeFromUtc(value.Date, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));
            return DB.News.AddUpdateNews(value);
        }

        [HttpDelete]
        [Route("api/Events/{eventID}")]
        public bool Delete(int eventID)
        {
            return DB.News.DeleteNews(eventID);
        }
    }
}
