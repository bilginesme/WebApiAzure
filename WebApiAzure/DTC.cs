using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace WebApiAzure
{
    public class DTC
    {
        public enum StatusEnum : int { Running = 1, Success = 2, Fail = 3 }
        public enum SizeEnum : int { Zero = 0, Small = 1, Medium = 2, Large = 3, Huge = 5, Gigantic = 9, Astronomical = 17 }

        public static bool IsNumeric(object input)
        {
            if (input == null) return false;

            string text = input.ToString();
            bool result = false;

            if (text.Length > 0)
            {
                char[] acceptedChars = "0123456789,.-'".ToCharArray();
                result = true; // innocent until proven guilty

                // look for the first non-numeric character in the input string
                for (int i = 0; i < text.Length; i++)
                {
                    // if the character is NOT in the list of valid characters, it is not a number
                    if (text.LastIndexOfAny(acceptedChars, i, 1) < 0)
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }
        public static string CommaToDot(string strValue)
        {
            string result = strValue;
            string groupSeperator = "";
            string decimalSeperator = "";

            decimalSeperator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            groupSeperator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator;

            result = result.Replace(groupSeperator, "");
            result = result.Replace(decimalSeperator, ".");

            return result;
        }
        public static string CommaToDot(object value)
        {
            if (IsNumeric(value))
            {
                return CommaToDot(value.ToString());
            }
            else
            {
                return "";
            }
        }

        public static string InputText(string text, int maxLength)
        {
            if (text == null) return "";

            text = text.Trim();
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (text.Length > maxLength)
                text = text.Substring(0, maxLength);

            text = Regex.Replace(text, "[\\s]{2,}", " ");  //two or more spaces
            text = Regex.Replace(text, "(<[b|B][r|R]/*>)+|(<[p|P](.|\\n)*?>)", "\n");      //<br>
            text = Regex.Replace(text, "(\\s*&[n|N][b|B][s|S][p|P];\\s*)+", " "); //&nbsp;
            text = Regex.Replace(text, "<(.|\\n)*?>", string.Empty);   //any other tags
            text = text.Replace("'", "''");

            return text;
        }
        public static string InputTextLight(string text, int maxLength)
        {
            if (text == null) return "";

            text = text.Trim();
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (text.Length > maxLength)
                text = text.Substring(0, maxLength);

            text = text.Replace("'", "''");

            return text;
        }

        public static string Format2(decimal num)
        {
            return String.Format("{0:##,#0.00}", num);
        }
        public static string Format2(int num)
        {
            return Format2((decimal)num);
        }
        public static string Format2(float num)
        {
            return Format2((decimal)num);
        }

        public static string Format1(decimal num)
        {
            return String.Format("{0:##,#0.0}", num);
        }
        public static string Format1(int num)
        {
            return Format1((decimal)num);
        }
        public static string Format1(float num)
        {
            return Format1((decimal)num);

        }
        public static string GetFullDigits(int value, int numDigits)
        {
            string strValue = value.ToString();

            while (strValue.Length < numDigits)
            {
                strValue = "0" + strValue;
            }

            return strValue;
        }

        public static string GetSmartDateTime(DateTime theDate, bool addTime)
        {
            string strResult = theDate.Day + " " +
                System.Globalization.DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(theDate.Month) + " " +
                +theDate.Year ;
            if (addTime) strResult += " " + GetHour(theDate, false);

            return strResult;
        }
        public static string GetHour(DateTime theDate, bool includeSeconds)
        {
            string theHour = "", theMinute = "";
            theHour = theDate.Hour.ToString();
            if (theHour.Length < 2) theHour = "0" + theHour;
            theMinute = theDate.Minute.ToString();
            if (theMinute.Length < 2) theMinute = "0" + theMinute;

            if (includeSeconds) return theHour + ":" + theMinute + ":00";
            else return theHour + ":" + theMinute;
        }
        public static string ObtainGoodDT(DateTime theDate, bool onlyDate)
        {
            string result = "";
            string theDay = "", theMonth = "", theYear = "";

            theDay = theDate.Day.ToString();
            if (theDay.Length < 2) theDay = "0" + theDay;
            theMonth = theDate.Month.ToString();
            if (theMonth.Length < 2) theMonth = "0" + theMonth;
            theYear = theDate.Year.ToString();

            if (onlyDate) result = "{ ts '" + theYear + "-" + theMonth + "-" + theDay + " 00:00:00' }";
            else result = "{ ts '" + theYear + "-" + theMonth + "-" + theDay + " " + GetHour(theDate, true) + "' }";

            return result;
        }
        public static DateTime GetDateTimeFromStringUS(string strDateTime)
        {
            DateTime dateTime = DateTime.MinValue;

            if (strDateTime.Length == 16)
            {
                int month = Convert.ToInt16(strDateTime.Substring(0, 2));
                int day = Convert.ToInt16(strDateTime.Substring(3, 2));
                int year = Convert.ToInt16(strDateTime.Substring(6, 4));

                int hour = Convert.ToInt16(strDateTime.Substring(11, 2));
                int minute = Convert.ToInt16(strDateTime.Substring(14, 2));

                dateTime = new DateTime(year, month, day, hour, minute, 0);
            }

            return dateTime;
        }
        public static string GetDateStringUS(DateTime dt)
        {
            string strYear = dt.Year.ToString();
            string strMonth = dt.Month.ToString();
            if (strMonth.Length == 1) strMonth = "0" + strMonth;
            string strDay = dt.Day.ToString();
            if (strDay.Length == 1) strDay = "0" + strDay;

            return strMonth + "/" + strDay + "/" + strYear;
            //return "01/01/2016";
        }
        public static string GetDateStringTR(DateTime dt)
        {
            string strYear = dt.Year.ToString();
            string strMonth = dt.Month.ToString();
            if (strMonth.Length == 1) strMonth = "0" + strMonth;
            string strDay = dt.Day.ToString();
            if (strDay.Length == 1) strDay = "0" + strDay;

            return strDay + "." + strMonth + "." + strYear;
            //return "01/01/2016";
        }
        public static string GetTimeStampForPDFCreation(DateTime dt)
        {
            string strYear = dt.Year.ToString();
            string strMonth = dt.Month.ToString();
            if (strMonth.Length == 1) strMonth = "0" + strMonth;
            string strDay = dt.Day.ToString();
            if (strDay.Length == 1) strDay = "0" + strDay;
            string strHour = dt.Hour.ToString();
            if (strHour.Length == 1) strHour = "0" + strHour;
            string strMinute = dt.Minute.ToString();
            if (strMinute.Length == 1) strMinute = "0" + strMinute;
            string strSecond = dt.Second.ToString();
            if (strSecond.Length == 1) strSecond = "0" + strSecond;

            return strYear + strMonth + strDay + "-" + strHour + strMinute + strSecond;
        }

        public static int GetWeekNumber(DateTime theDate)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("sv-SE", false);
            int result = culture.Calendar.GetWeekOfYear(theDate, System.Globalization.CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday);

            return result;
        }
        /// <summary>
        /// Returns the next week
        /// </summary>
        /// <param name="presentWeek">Present Week</param>
        /// <returns>Next Week</returns>
        public static WeekInfo GetNextWeek(WeekInfo presentWeek)
        {
            return new WeekInfo(presentWeek.StartDate.AddDays(7));
        }
        /// <summary>
        /// Returns the previous week
        /// </summary>
        /// <param name="presentWeek">Present Week</param>
        /// <returns>Next Week</returns>
        public static WeekInfo GetPreviousWeek(WeekInfo presentWeek)
        {
            return new WeekInfo(presentWeek.StartDate.AddDays(-7));
        }
    }
}