using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ChapterInfo
    {
        #region Enums
        public enum LevelEnum : int
        {
            Zero = 0, First = 1, Second = 2, Third = 3
        }
        #endregion

        #region Private Members
        int id;
        int order;
        int masterID;
        int bookID;
        string title;
        string details;
        int estMin;
        LevelEnum level;
        #endregion

        #region Constructors
        public ChapterInfo()
        {
            id = 0;
            masterID = 0;
            order = 0;
            bookID = 0;
            title = "";
            details = "";
            estMin = 0;
            level = LevelEnum.First;
        }
        #endregion

        #region Public Properties
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public int MasterID
        {
            get { return masterID; }
            set { masterID = value; }
        }
        public int Order
        {
            get { return order; }
            set { order = value; }
        }
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        public string Details
        {
            get { return details; }
            set { details = value; }
        }
        public int EstMin
        {
            get { return estMin; }
            set { estMin = value; }
        }
        public int BookID
        {
            get { return bookID; }
            set { bookID = value; }
        }
        public LevelEnum Level
        {
            get { return level; }
            set { level = value; }
        }
        #endregion
    }
}