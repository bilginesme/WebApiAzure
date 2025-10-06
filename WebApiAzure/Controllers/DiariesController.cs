using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class DiariesController : ApiController
    {
        [HttpGet]
        [Route("api/Diaries/")]
        public IEnumerable<DiaryInfo> Get()
        {
            return new List<DiaryInfo>();
        }

        [HttpGet]
        [Route("api/Diaries/{strDateStart}/{strDateEnd}/{natureID}")]
        public IEnumerable<DiaryInfo> Get(string strDateStart, string strDateEnd, int natureID)
        {
            List<DiaryInfo> data = new List<DiaryInfo>();

            DateTime dtStart = DateTime.Today;
            DateTime dtEnd = DateTime.Today;

            DiaryInfo.NatureEnum nature = (DiaryInfo.NatureEnum)natureID; 

            if (strDateStart != string.Empty)
                dtStart = DTC.Date.GetDateFromString(strDateStart, DTC.Date.DateStyleEnum.Universal);

            if (strDateEnd != string.Empty)
                dtEnd = DTC.Date.GetDateFromString(strDateEnd, DTC.Date.DateStyleEnum.Universal);

            data = DB.Diary.GetDiaries(dtStart, dtEnd, nature);

            return data;
        }

        [HttpGet]
        [Route("api/Diaries/{natureID}/{objectID}")]
        public IEnumerable<DiaryInfo> Get(int natureID, int objectID)
        {
            List<DiaryInfo> data = new List<DiaryInfo>();

            DiaryInfo.NatureEnum nature = (DiaryInfo.NatureEnum)natureID;

            if(nature == DiaryInfo.NatureEnum.Project)
            {
                data = DB.Diary.GetDiariesOfProject(objectID);
            }

            return data;
        }

        [HttpGet]
        [Route("api/Diaries/{parameter1}/{parameter2}/{parameter3}/{parameter4}")]
        public IEnumerable<DiaryInfo> Get(int parameter1, string parameter2, string parameter3, string parameter4)
        {
            List<DiaryInfo> data = new List<DiaryInfo>();

            if(parameter1 == 1)
            {
                string strProjectIDs = parameter2;
                data = DB.Diary.GetLatestDiariesOfProjects(strProjectIDs);

            }
     
            return data;
        }

        [HttpGet]
        [Route("api/Diaries/{id}")]
        public DiaryInfo Get(int id)
        {
            return DB.Diary.GetDiary(id);
        }

        [HttpPost]
        [Route("api/Diaries/")]
        public long Post([FromBody] DiaryInfo value)
        {
            return DB.Diary.AddUpdateDiary(value);
        }

        [HttpPut]
        [Route("api/Diaries/{diaryID}")]
        public long Put(int diaryID, [FromBody] DiaryInfo value)
        {
            return DB.Diary.AddUpdateDiary(value);
        }

        [HttpDelete]
        [Route("api/Diaries/{diaryID}")]
        public bool Delete(long diaryID)
        {
            return DB.Diary.DeleteDiary(diaryID);
        }
    }
}
