using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class TimeInfo
    {
        #region Private Members
        int hour, minute;
        #endregion

        #region Constructors
        public TimeInfo()
        {
            hour = 0;
            minute = 0;
        }
        public TimeInfo(int hour, int minute)
        {
            this.hour = hour;
            this.minute = minute;

            if (hour > 23) hour = 23;
            if (hour < 0) hour = 0;
            if (minute > 59) minute = 59;
            if (minute < 0) minute = 0;
        }
        public TimeInfo(string timeString)
        {
            hour = minute = 0;

            if (timeString.Length == 5)
            {
                if (DTC.IsNumeric(timeString.Substring(0, 2))
                    && DTC.IsNumeric(timeString.Substring(3, 2)))
                {
                    hour = Convert.ToInt32(timeString.Substring(0, 2));
                    minute = Convert.ToInt32(timeString.Substring(3, 2));
                }
            }

            if (hour > 23) hour = 23;
            if (hour < 0) hour = 0;
            if (minute > 59) minute = 59;
            if (minute < 0) minute = 0;
        }
        #endregion

        #region Public Methods
        public string GetTimeString()
        {
            string sHour = "";
            string sMinute = "";

            sHour = hour.ToString();
            if (sHour.Length == 1) sHour = "0" + sHour;
            sMinute = minute.ToString();
            if (sMinute.Length == 1) sMinute = "0" + sMinute;

            return sHour + ":" + sMinute;
        }
        public float GetDecimalValue()
        {
            return (float)hour + (float)minute / 60;
        }
        public string GetHour()
        {
            string sHour = "";

            sHour = hour.ToString();
            if (sHour.Length == 1) sHour = "0" + sHour;

            return sHour;
        }
        public string GetMinute()
        {
            string sMinute = "";

            sMinute = minute.ToString();
            if (sMinute.Length == 1) sMinute = "0" + sMinute;

            return sMinute;
        }
        #endregion

        #region Public Properties
        public int Hour
        {
            get { return hour; }
            set { hour = value; }
        }
        public int Minute
        {
            get { return minute; }
            set { minute = value; }
        }
        #endregion
    }
}