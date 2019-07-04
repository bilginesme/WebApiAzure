using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class DaysController : ApiController
    {
        [HttpGet]
        [Route("api/Days/")]
        public IEnumerable<DayInfo> Get()
        {
            return new List<DayInfo>();
        }

        [HttpGet]
        [Route("api/Days/{strDateStart}/{strDateEnd}/{isCreate}")]
        public IEnumerable<DayInfo> Get(string strDateStart, string strDateEnd, bool isCreate)
        {
            DateTime dtStart = DateTime.Today;
            DateTime dtEnd = DateTime.Today;

            if (strDateStart != string.Empty)
                dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);

            if (strDateEnd != string.Empty)
                dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            return DB.Days.GetDays(dtStart, dtEnd, isCreate);
        }

        [HttpGet]
        [Route("api/Days/{dayID}")]
        public DayInfo Get(int dayID)
        {
            return DB.Days.GetDay(dayID);
        }

        [HttpGet]
        [Route("api/Days/{parameter}/{strDate}")]
        public DayInfo Get(int parameter, string strDate)
        {
            DateTime theDate = DateTime.Today;

            if (strDate != string.Empty)
                theDate = DTC.Date.GetDateFromString(strDate, DTC.Date.DateStyleEnum.Universal);

            return DB.Days.GetDay(theDate, true);
        }

        [HttpPost]
        [Route("api/Days/")]
        public void Post([FromBody]DayInfo value)
        {
        }

        [HttpPut]
        [Route("api/Days/{dayID}")]
        public void Put(int dayID, [FromBody]DayInfo value)
        {
        }

        [HttpDelete]
        [Route("api/Days/{dayID}")]
        public void Delete(int dayID)
        {
        }
    }
}
