using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class RealHoursPerWeekInfo
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public decimal RealHours { get; set; }

        public RealHoursPerWeekInfo(int Year, int Week, decimal RealHours)
        {
            this.Year = Year;
            this.Week = Week;
            this.RealHours = RealHours;
        }
    }
}