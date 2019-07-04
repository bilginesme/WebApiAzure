using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class WeeklyPerformanceInfo
    {
        public string Label { get; set; }
        public float Percentage { get; set; }

        public WeeklyPerformanceInfo()
        {
            Label = string.Empty;
            Percentage = 0;
        }
    }
}   