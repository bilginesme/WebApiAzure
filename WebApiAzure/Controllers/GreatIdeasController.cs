using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class GreatIdeasController : ApiController
    {
        // GET: api/GreatIdeas
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("api/GreatIdeas/{parameter}/{strDateStart}/{strDateEnd}/{numIdeas}")]
        public List<IdeaInfo> Get(int parameter, string strDateStart, string strDateEnd, int numIdeas)
        {
            DateTime dtStart = DateTime.Today;
            DateTime dtEnd = DateTime.Today;

            if (strDateStart != string.Empty)
                dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);

            if (strDateEnd != string.Empty)
                dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            return DB.Ideas.GetIdeas(dtStart, dtEnd, DTC.SizeEnum.Huge);
        }


        // GET: api/GreatIdeas/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/GreatIdeas
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/GreatIdeas/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/GreatIdeas/5
        public void Delete(int id)
        {
        }
    }
}
