using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class BookProcessInfo
    {
        #region Enums
        public enum BookProcessType : int
        {
            Standart = 1, ChapterBased = 2, EffectiveReading = 3
        }
        public enum DestinationEnum : int
        {
            NoDestination = 0, Project = 1, Block = 2, Segment = 3
        }
        #endregion

        #region Private Members
        int id;
        int bookID;
        BookProcessType processType;
        DTC.StatusEnum status;
        int currentPage;
        int currentDuration;
        DateTime startDate;
        DateTime endDate;
        string details;
        #endregion

        #region Constructors
        public BookProcessInfo()
        {
            id = 0;
            bookID = 0;
            status = DTC.StatusEnum.Running;
            processType = BookProcessType.Standart;
            currentPage = 0;
            currentDuration = 0;
            startDate = DateTime.Today;
            endDate = DateTime.Today;
            details = "";
        }
        #endregion

        #region Public Properties
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public int BookID
        {
            get { return bookID; }
            set { bookID = value; }
        }
        public BookProcessType ProcessType
        {
            get { return processType; }
            set { processType = value; }
        }
        public DTC.StatusEnum Status
        {
            get { return status; }
            set { status = value; }
        }
        public int CurrentPage
        {
            get { return currentPage; }
            set { currentPage = value; }
        }
        public int CurrentDuration
        {
            get { return currentDuration; }
            set { currentDuration = value; }
        }
        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }
        public string Details
        {
            get { return details; }
            set { details = value; }
        }
        #endregion
    }
}