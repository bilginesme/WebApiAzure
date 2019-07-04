using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using WebApiAzure.Models;

namespace WebApiAzure
{
    public class DTC
    {
        public enum StatusEnum : int { NA = 0, Running = 1, Success = 2, Fail = 3 }
        public enum RankEnum : int { NoRank = 0, RankA = 1, RankB = 2, RankC = 3 }
        public enum SizeEnum : int { Zero = 0, Small = 1, Medium = 2, Large = 3, Huge = 5, Gigantic = 9, Astronomical = 17 }
        public enum RangeEnum : int { Floating = 0, Day = 1, Week = 2, Month = 3, Quarter = 4, Year = 5, Decade = 6, Lifetime = 7 }
        public enum NextPrevEnum { Next, Previous, Today }
        public enum BookNature : int { TypeFree = 0, Book = 1, Article = 2, Audiobook = 3 }
        public enum BookCountMethod : int
        {
            Standart = 1, Weighted = 2
        }


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

        public class Date
        {
            public enum DateStyleEnum
            {
                American, European, Universal
            }

            public static DateTime GetMin()
            {
                return new DateTime(1900, 1, 1);
            }
            public static bool IsMin(DateTime date)
            {
                if (date.Year == 1900 && date.Month == 1 && date.Day == 1) return true;
                else return false;
            }
            /// <summary>
            /// Returns a standart "european" style date format (dd.mmmmm.yyyy)
            /// </summary>
            /// <param name="theDate">Date value to format</param>
            /// <returns>A string representing the date value</returns>
            public static string GetStandartDate(DateTime theDate)
            {
                string theDay = "", theMonth = "", theYear = "";

                theDay = theDate.Day.ToString();
                if (theDay.Length < 2) theDay = "0" + theDay;
                theMonth = theDate.Month.ToString();
                if (theMonth.Length < 2) theMonth = "0" + theMonth;
                theYear = theDate.Year.ToString();

                return theDay + "." + theMonth + "." + theYear;
            }
            /// <summary>
            /// Returns a standart date format (ddmmmmmyyyy)
            /// </summary>
            /// <param name="theDate">Date value</param>
            /// <returns>A unique code representing the date value</returns>
            public static string GetCodestring(DateTime theDate)
            {
                string theDay = "", theMonth = "", theYear = "";

                theDay = theDate.Day.ToString();
                if (theDay.Length < 2) theDay = "0" + theDay;
                theMonth = theDate.Month.ToString();
                if (theMonth.Length < 2) theMonth = "0" + theMonth;
                theYear = theDate.Year.ToString();

                return theDay + theMonth + theYear;
            }
            /// <summary>
            /// Extracts date from a sring in this format
            /// dd.mm.yyyy
            /// </summary>
            /// <param name="strDate">Date string in format dd.mm.yyyy</param>
            /// <returns>Date</returns>
            public static DateTime GetDateFromString(string strDate, DateStyleEnum dateStyle)
            {
                DateTime result = DateTime.MinValue;

                char[] sepAM = { '/' };
                char[] sepEU = { '.' };
                char[] sepUNI = { '-' };

                string[] str = { "" };

                if (dateStyle == DateStyleEnum.American) str = strDate.Split(sepAM, StringSplitOptions.None);
                else if (dateStyle == DateStyleEnum.European) str = strDate.Split(sepEU, StringSplitOptions.None);
                else if (dateStyle == DateStyleEnum.Universal) str = strDate.Split(sepUNI, StringSplitOptions.None);

                if (str.Length == 3)
                {
                    int day = 0;
                    int month = 0;
                    int year = 0;
                    if (DTC.IsNumeric(str[0]))
                    {
                        if (dateStyle == DateStyleEnum.American) month = Convert.ToInt16(str[0]);
                        else if (dateStyle == DateStyleEnum.European) day = Convert.ToInt16(str[0]);
                        else if (dateStyle == DateStyleEnum.Universal) year = Convert.ToInt16(str[0]);
                    }
                    if (DTC.IsNumeric(str[1]))
                    {
                        if (dateStyle == DateStyleEnum.American) day = Convert.ToInt16(str[1]);
                        if (dateStyle == DateStyleEnum.European) month = Convert.ToInt16(str[1]);
                        if (dateStyle == DateStyleEnum.Universal) month = Convert.ToInt16(str[1]);
                    }
                    if (DTC.IsNumeric(str[2]))
                    {
                        if (dateStyle == DateStyleEnum.American) year = Convert.ToInt16(str[2]);
                        else if (dateStyle == DateStyleEnum.European) year = Convert.ToInt16(str[2]);
                        else if (dateStyle == DateStyleEnum.Universal) day = Convert.ToInt16(str[2]);
                    }

                    if (day > 0 && month > 0 && year > 0)
                    {
                        result = new DateTime(year, month, day);
                    }
                }

                return result;

            }
            public static string GetSmartDate(DateTime theDate, bool addYear)
            {
                string result = "";

                result = theDate.Day + " " +
                    System.Globalization.DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(theDate.Month) + " " +
                    System.Globalization.DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(theDate.DayOfWeek);
                if (addYear) result += " " + theDate.Year.ToString().Substring(2, 2);
                return result;
            }
            public static string GetSmartDateShort(DateTime theDate)
            {
                string result = "";

                result = theDate.Day + " " +
                    System.Globalization.DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(theDate.Month) +
                    " " + theDate.Year.ToString().Substring(2, 2);
                return result;
            }
            public static string GetWeekDay(DateTime date, bool isShort)
            {
                if (isShort)
                    return System.Globalization.DateTimeFormatInfo.CurrentInfo.GetShortestDayName(date.DayOfWeek);
                else
                    return System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName(date.DayOfWeek);
            }
            /// <summary>
            /// Returns an SQL Friendly datetime string.
            /// </summary>
            /// <param name="theDate">Date value</param>
            /// <param name="onlyDate">If only the date value is required, choose "true" [dd.mm.yyyy]. 
            /// If time values will be added, choose "false" [dd.mm.yyyy hh:mm:ss].</param>
            /// <returns>The SQL friendly string</returns>
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
            /// <summary>
            /// Returns a clean time string [hh:mm:ss]
            /// </summary>
            /// <param name="theDate">The date value (that will include the time)</param>
            /// <param name="includeSeconds">If true, seconds will be added to the string</param>
            /// <returns>The time string</returns>
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
            /// <summary>
            /// Returns the next month
            /// </summary>
            /// <param name="presentMonth">Present Month</param>
            /// <returns></returns>
            public static MonthInfo GetNextMonth(MonthInfo presentMonth)
            {
                int month = presentMonth.Month;
                int year = presentMonth.Year;

                month++;

                if (month > 12)
                {
                    year++;
                    month = 1;
                }

                return new MonthInfo(new DateTime(year, month, 1));
            }
            /// <summary>
            /// Returns the previous month
            /// </summary>
            /// <param name="presentMonth">Present Month</param>
            /// <returns>Previous Month</returns>
            public static MonthInfo GetPreviousMonth(MonthInfo presentMonth)
            {
                int month = presentMonth.Month;
                int year = presentMonth.Year;

                month--;

                if (month < 1)
                {
                    year--;
                    month = 12;
                }

                return new MonthInfo(new DateTime(year, month, 1));
            }
            /// <summary>
            /// Returns the week of the year
            /// </summary>
            /// <param name="theDate">The reference date</param>
            /// <returns>Week number [1-52]</returns>
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
            public static QuarterInfo GetNextQuarter(QuarterInfo presentQuarter)
            {
                int quarterNo = (int)presentQuarter.Quarter;
                int year = presentQuarter.Year;

                quarterNo++;

                if (quarterNo > (int)QuarterInfo.QuarterEnum.Fourth)
                {
                    year++;
                    quarterNo = (int)QuarterInfo.QuarterEnum.First;
                }

                return new QuarterInfo((QuarterInfo.QuarterEnum)quarterNo, year);
            }
            public static QuarterInfo GetPreviousQuarter(QuarterInfo presentQuarter)
            {
                int quarterNo = (int)presentQuarter.Quarter;
                int year = presentQuarter.Year;

                quarterNo--;

                if (quarterNo < (int)QuarterInfo.QuarterEnum.First)
                {
                    year--;
                    quarterNo = (int)QuarterInfo.QuarterEnum.Fourth;
                }

                return new QuarterInfo((QuarterInfo.QuarterEnum)quarterNo, year);
            }
            public static string GetSmartHours(int min)
            {
                string result = "";

                int h = 0;
                int m = 0;

                h = min / 60;
                m = min - h * 60;

                if (h > 0 && m > 0)
                    result = h + "h" + m + "m";
                else if (h == 0 && m > 0)
                    result = m + "m";
                else if (h > 0 && m == 0)
                    result = h + "h";

                return result;
            }
            public static TimeSpan GetTimeSpan(int minutes)
            {
                int hours = minutes / 60;
                minutes = minutes - hours * 60;

                return new TimeSpan(hours, minutes, 0);
            }
            public static void GetNextPrevDates(DTC.RangeEnum range, DateTime date, NextPrevEnum np, out DateTime startDate, out DateTime endDate)
            {
                startDate = DateTime.Today;
                endDate = DateTime.Today;

                if (range == DTC.RangeEnum.Day)
                {
                    if (np == NextPrevEnum.Next)
                    {
                        startDate = date.AddDays(1);
                        endDate = startDate;
                    }
                    if (np == NextPrevEnum.Previous)
                    {
                        startDate = date.AddDays(-1);
                        endDate = startDate;
                    }
                    if (np == NextPrevEnum.Today)
                    {
                        startDate = date;
                        endDate = startDate;
                    }
                }
                else if (range == DTC.RangeEnum.Week)
                {
                    WeekInfo week = new WeekInfo(date);

                    if (np == NextPrevEnum.Next)
                    {
                        startDate = GetNextWeek(week).StartDate;
                        endDate = GetNextWeek(week).EndDate;
                    }
                    if (np == NextPrevEnum.Previous)
                    {
                        startDate = GetPreviousWeek(week).StartDate;
                        endDate = GetPreviousWeek(week).EndDate;
                    }
                    if (np == NextPrevEnum.Today)
                    {
                        startDate = week.StartDate;
                        endDate = week.EndDate;
                    }
                }
                else if (range == DTC.RangeEnum.Month)
                {
                    MonthInfo month = new MonthInfo(date);

                    if (np == NextPrevEnum.Next)
                    {
                        startDate = GetNextMonth(month).StartDate;
                        endDate = GetNextMonth(month).EndDate;
                    }
                    if (np == NextPrevEnum.Previous)
                    {
                        startDate = GetPreviousMonth(month).StartDate;
                        endDate = GetPreviousMonth(month).EndDate;
                    }
                    if (np == NextPrevEnum.Today)
                    {
                        startDate = month.StartDate;
                        endDate = month.EndDate;
                    }
                }
                else if (range == DTC.RangeEnum.Quarter)
                {
                    QuarterInfo quarter = new QuarterInfo(date);

                    if (np == NextPrevEnum.Next)
                    {
                        startDate = GetNextQuarter(quarter).StartDate;
                        endDate = GetNextQuarter(quarter).EndDate;
                    }
                    if (np == NextPrevEnum.Previous)
                    {
                        startDate = GetPreviousQuarter(quarter).StartDate;
                        endDate = GetPreviousQuarter(quarter).EndDate;
                    }
                    if (np == NextPrevEnum.Today)
                    {
                        startDate = quarter.StartDate;
                        endDate = quarter.EndDate;
                    }
                }
                else if (range == DTC.RangeEnum.Year)
                {
                    if (np == NextPrevEnum.Next)
                    {
                        startDate = new YearInfo(date.Year + 1).StartDate;
                        endDate = new YearInfo(date.Year + 1).EndDate;
                    }
                    if (np == NextPrevEnum.Previous)
                    {
                        startDate = new YearInfo(date.Year - 1).StartDate;
                        endDate = new YearInfo(date.Year - 1).EndDate;
                    }
                    if (np == NextPrevEnum.Today)
                    {
                        startDate = new YearInfo(date.Year).StartDate;
                        endDate = new YearInfo(date.Year).EndDate;
                    }
                }
            }
        }
        public class Control
        {
            /// <summary>
            /// Method to make sure that user's inputs are not malicious
            /// </summary>
            /// <param name="text">User's Input</param>
            /// <param name="maxLength">Maximum length of input</param>
            /// <returns>The cleaned up version of the input</returns>
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
            public static string InputText(string text)
            {
                if (text == null) return "";

                text = text.Trim();
                if (string.IsNullOrEmpty(text))
                    return string.Empty;

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
            public static bool IsString(string theString)
            {
                if (theString.Trim().Length > 0) return true;
                else return false;
            }
            public static int GetNumericArgument(string arg)
            {
                int result = 0;

                if (DTC.IsNumeric(arg))
                {
                    result = Convert.ToInt32(arg);
                }

                return result;
            }
        }

        public class DTMath
        {
            public static double NormalDist(double x, double mean, double stdev)
            {
                return Math.Exp(-(Math.Pow((x - mean) / stdev, 2) / 2)) / Math.Sqrt(2 * Math.PI) / stdev;
            }
            public static float GradualDecline(float value)
            {
                if (value < 0) return 1;
                else if (value >= 0 && value < Math.E)
                {
                    return 1 - (float)Math.Log(value + 1, Math.E + 1);
                }
                else if (value >= Math.E)
                {
                    return 0;
                }
                else
                    return 0;
            }
        }

        public class Info
        {
            public static ProjectInfo Get(Dictionary<int, ProjectInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new ProjectInfo();
            }
            public static IdeaInfo Get(Dictionary<int, IdeaInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new IdeaInfo();
            }
            public static IdeaGroupInfo Get(Dictionary<int, IdeaGroupInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new IdeaGroupInfo();
            }
            public static ProjectGroupInfo Get(Dictionary<int, ProjectGroupInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new ProjectGroupInfo();
            }
            public static ProjectTypeInfo Get(Dictionary<int, ProjectTypeInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new ProjectTypeInfo();
            }
            public static PragmaInfo Get(Dictionary<int, PragmaInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new PragmaInfo();
            }
            public static PragmaAttributeInfo Get(Dictionary<int, PragmaAttributeInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new PragmaAttributeInfo();
            }
            public static PragmaInstanceInfo Get(Dictionary<int, PragmaInstanceInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new PragmaInstanceInfo(new PragmaInfo());
            }
            public static NewsInfo Get(Dictionary<int, NewsInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new NewsInfo();
            }
            public static GoalInfo Get(Dictionary<int, GoalInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new GoalInfo();
            }
            public static DiaryInfo Get(Dictionary<int, DiaryInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new DiaryInfo();
            }
            public static TaskInfo Get(Dictionary<int, TaskInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new TaskInfo();
            }
            public static BlockInfo Get(Dictionary<int, BlockInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new BlockInfo();
            }
            public static SegmentInfo Get(Dictionary<int, SegmentInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new SegmentInfo();
            }
            public static GoalGroupInfo Get(Dictionary<int, GoalGroupInfo> dict, object key)
            {
                int id = 0;
                if (DTC.IsNumeric(key)) id = Convert.ToInt32(key);

                if (dict.ContainsKey(id))
                    return dict[id];
                else
                    return new GoalGroupInfo();
            }
            public static int Index(List<DayInfo> lst, DayInfo item)
            {
                int result = -1;

                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].DayID == item.DayID) result = i;
                }



                return result;
            }
        }
    }
}