using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class WeeksController : ApiController
    {
        [HttpGet]
        [Route("api/Weeks/")]
        public IEnumerable<WeekInfo> Get()
        {
            return new List<WeekInfo>();
        }

        [HttpGet]
        [Route("api/Weeks/{strDateStart}/{strDateEnd}/{isCreate}")]
        public IEnumerable<WeekInfo> Get(string strDateStart, string strDateEnd, bool isCreate)
        {
            DateTime dtStart = DateTime.Today;
            DateTime dtEnd = DateTime.Today;

            if (strDateStart != string.Empty)
                dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);

            if (strDateEnd != string.Empty)
                dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            return DB.Weeks.GetWeeks(dtStart, dtEnd, isCreate);
        }

        [HttpGet]
        [Route("api/Weeks/{weekID}")]
        public WeekInfo Get(int weekID)
        {
            return DB.Weeks.GetWeek(weekID);
        }

        [HttpGet]
        [Route("api/Weeks/{parameter}/{strDate}")]
        public WeekInfo Get(int parameter, string strDate)
        {
            DateTime theDate = DateTime.Today;

            if (strDate != string.Empty)
                theDate = DTC.Date.GetDateFromString(strDate, DTC.Date.DateStyleEnum.Universal);

            return DB.Weeks.GetWeek(theDate, true);
        }

        [HttpPost]
        [Route("api/Weeks/")]
        public void Post([FromBody]WeekInfo value)
        {
        }

        [HttpPut]
        [Route("api/Weeks/{weekID}")]
        public void Put(int weekID, [FromBody]WeekInfo value)
        {
        }

        [HttpDelete]
        [Route("api/Weeks/{weekID}")]
        public void Delete(int weekID)
        {
        }
    }
}
