using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class BookInfo
    {
        public enum AudiobookProcessTypeEnum { NA = 0, Hours = 1, Minutes = 2, Chapters = 3 }

        #region Public Members
        public int ID { get; set; }
        public DTC.BookNature Nature { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public float TotalValue { get; set; }
        public float CurrentValue { get; set; }
        public bool IsTrackProgress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DTC.StatusEnum Status { get; set; }
        public string Details { get; set; }
        public AudiobookProcessTypeEnum AudiobookProcessType { get; set; }
        #endregion

        #region Constructors
        public BookInfo()
        {
            ID = 0;
            Nature = DTC.BookNature.Book;
            Title = string.Empty;
            Author = string.Empty;
            TotalValue = 0;
            CurrentValue = 0;
            IsTrackProgress = true;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            Status = DTC.StatusEnum.Running;
            Details = string.Empty;
            AudiobookProcessType = AudiobookProcessTypeEnum.NA;
        }
        #endregion

        #region Public Methods

        #endregion
    }
}   