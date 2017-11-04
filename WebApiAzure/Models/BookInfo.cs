using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class BookInfo
    {
        #region Private Members
        int id;
        DTC.BookNature nature;
        DTC.SizeEnum size;
        string title;
        string author;
        int numPages;
        int totalDuration;
        DateTime entryDate;
        string details;
        Dictionary<int, ChapterInfo> chapters;
        bool isProcessed;
        #endregion

        #region Constructors
        public BookInfo()
        {
            id = 0;
            nature = DTC.BookNature.Book;
            size = DTC.SizeEnum.Zero;
            title = "";
            author = "";
            numPages = 0;
            totalDuration = 0;
            entryDate = DateTime.Today;
            details = "";
            chapters = new Dictionary<int, ChapterInfo>();
            isProcessed = false;
        }
        #endregion

        #region Public Methods
        public float GetSize()
        {
            return (float)((int)size);
        }
        #endregion

        #region Public Properties
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public DTC.BookNature Nature
        {
            get { return nature; }
            set { nature = value; }
        }
        public DTC.SizeEnum Size
        {
            get { return size; }
            set { size = value; }
        }
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        public string Author
        {
            get { return author; }
            set { author = value; }
        }
        public int NumPages
        {
            get { return numPages; }
            set { numPages = value; }
        }
        public int TotalDuration
        {
            get { return totalDuration; }
            set { totalDuration = value; }
        }
        public DateTime EntryDate
        {
            get { return entryDate; }
            set { entryDate = value; }
        }
        public string Details
        {
            get { return details; }
            set { details = value; }
        }
        public Dictionary<int, ChapterInfo> Chapters
        {
            get { return chapters; }
            set { chapters = value; }
        }
        public bool IsProcessed
        {
            get { return isProcessed; }
            set { isProcessed = value; }
        }
        #endregion
    }
}