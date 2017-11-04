using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class DiaryInfo
    {
        #region Enums
        public enum NatureEnum : int
        {
            NA = 0, Project = 1, Day = 2, Week = 3, Month = 4, Quarter = 5, Year = 6
        }
        #endregion

        #region Private Members
        int id;
        string title;
        string body;
        DateTime date;
        NatureEnum nature;
        int objectID;
        #endregion

        #region Constructors
        public DiaryInfo()
        {
            id = 0;
            title = "";
            body = "";
            date = DateTime.Today;
            nature = NatureEnum.NA;
            objectID = 0;
        }
        #endregion

        #region Public Methods
        public int GetPassedDays()
        {
            TimeSpan ts = DateTime.Today.Subtract(date);
            return (int)ts.TotalDays;
        }
        #endregion

        #region Public Properties
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        public string Body
        {
            get { return body; }
            set { body = value; }
        }
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }
        public NatureEnum Nature
        {
            get { return nature; }
            set { nature = value; }
        }
        public int ObjectID
        {
            get { return objectID; }
            set { objectID = value; }
        }
        #endregion
    }
}