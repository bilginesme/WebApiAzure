using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class GoalInfo : ICloneable
    {
        #region Enums
        public enum NatureEnum : int { NA = 0, Standart = 1, BetweenLimits = 2 }
        public enum MeasurementStyleEnum : int { NA = 0, LastValue = 1, Average = 2 }
        #endregion

        #region Private Members
        long id;
        int groupID;
        bool isFocus;
        int templateID;
        DateTime startDate, endDate, dueDate;
        NatureEnum nature;
        DTC.RangeEnum range;
        int ownerID;
        DTC.StatusEnum status;
        DTC.SizeEnum size;
        GoalTypeInfo.TypeEnum goalType;
        string definition;
        string details;
        float startingValue;
        float goalValue;
        int primaryProjectID;
        int estimatedMinutes;
        float estimatedHours;
        float thresholdValue;
        float genericValue;
        int itemID;
        int projectGroupID;
        int projectID;
        int secondaryProjectGroupID;
        int secondaryProjectID;
        int tertiaryProjectGroupID;
        int tertiaryProjectID;
        string hour;
        int criteria;
        int measurementStyle;
        int itemNature;
        int pragmaID;
        int pragmaAttributeID;
        string pragmaAttributePhrase;
        float pragmaAttributeValue;
        int pragmaNumInstances;
        float presentValue = 0;
        bool isBlackAndWhite;
        #endregion

        #region Constructors
        public GoalInfo()
        {
            id = 0;
            groupID = 1;
            isFocus = false;
            templateID = 0;
            definition = "";
            details = "";
            nature = NatureEnum.Standart;
            range = DTC.RangeEnum.Floating;
            ownerID = 0;
            status = DTC.StatusEnum.Running;
            size = DTC.SizeEnum.Medium;
            startDate = DateTime.Now;
            endDate = DateTime.Now;
            dueDate = DateTime.Now;
            goalType = GoalTypeInfo.TypeEnum.NA;
            primaryProjectID = 0;
            estimatedMinutes = 0;
            estimatedHours = 0;
            thresholdValue = 0;
            itemID = 0;
            projectGroupID = 0;
            thresholdValue = 0;
            goalValue = 0;
            genericValue = 0;
            projectID = 0;
            secondaryProjectGroupID = 0;
            secondaryProjectID = 0;
            tertiaryProjectGroupID = 0;
            tertiaryProjectID = 0;
            hour = "00:00";
            criteria = 0;
            measurementStyle = 0;
            itemNature = 0;
            pragmaID = 0;
            pragmaAttributeID = 0;
            pragmaAttributePhrase = "";
            pragmaAttributeValue = 0;
            pragmaNumInstances = 0;
            isBlackAndWhite = false;
        }
        #endregion

        #region Private Methods
        private float GetPassedDays(DayInfo today)
        {
            float result = 0;

            TimeSpan upToNow = DateTime.Today.Subtract(startDate);
            result = (float)upToNow.Days;

            result += today.GetTodaysPassedHours() / today.GetTodaysTotalHours();

            if (result < 0) result = 0;
            if (result > GetTotalDays()) result = GetTotalDays();

            return result;
        }
        private float GetTotalDays()
        {
            float result = 0;

            TimeSpan allSpan = dueDate.Subtract(startDate);
            result = (float)allSpan.TotalDays + 1;
            if (result < 0) result = 0;

            return result;
        }
        #endregion

        #region Public Methods
        public bool IsNull() { return id > 0 ; }
        public float GetDesiredPercentage(DayInfo today)
        {
            float result = 0;

            if (GetTotalDays() != 0)
            {
                if (nature == NatureEnum.Standart)
                    result = 100 * GetPassedDays(today) / GetTotalDays();
                else if (nature == NatureEnum.BetweenLimits)
                    result = 100 * GetRemainingDays(today) / GetTotalDays();
            }
            else result = 0;

            if (result < 0) result = 0;
            else if (result > 100) result = 100;

            return result;
        }
        /// <summary>
        /// Number of days remaining to "due date".
        /// </summary>
        /// <returns>Number of days</returns>
        public float GetRemainingDays(DayInfo today)
        {
            float result = 0;

            TimeSpan leftDays = dueDate.Subtract(DateTime.Today);
            result = (float)leftDays.Days;

            result += (today.GetTodaysTotalHours() - today.GetTodaysPassedHours()) / today.GetTodaysTotalHours();

            if (result < 0) result = 0;
            if (result > GetTotalDays()) result = GetTotalDays();

            return result;
        }
        /// <summary>
        /// When tracking goals, present value should be compared to "desired value".
        /// This method calculates the desired value.
        /// </summary>
        /// <returns>The desired value.</returns>
        public float GetDesiredValue(DayInfo today)
        {
            float result = 0;

            if (goalValue > 0)
            {
                if (nature == NatureEnum.Standart)
                    result = startingValue - GetDesiredPercentage(today) * (startingValue - goalValue) / 100;
                else if (nature == NatureEnum.BetweenLimits)
                    result = startingValue - (100 - GetDesiredPercentage(today)) * (startingValue - goalValue) / 100;
                //result = startingValue - (desiredPercentage) * (startingValue - goalValue) / 100;

            }
            else result = 0;

            return result;
        }
        /// <summary>
        /// Gets the present percentage of goal completion.
        /// </summary>
        /// <returns>The percentage</returns>
        public float GetPresentPercentage()
        {
            float result = 0;

            if(status == DTC.StatusEnum.Success)
            {
                result = 100;
            }
            else
            {
                if (goalValue > 0)
                {
                    if (nature == NatureEnum.Standart)
                        result = 100 * (presentValue - startingValue) / (goalValue - startingValue);
                    else if (nature == NatureEnum.BetweenLimits)
                        result = 100 * (goalValue - presentValue) / goalValue;
                }
                else result = 0;

                if (result > 100) result = 100;
                if (result < 0) result = 0;
            }

            return result;
        }
        /// <summary>
        /// This method gives the daily suggested value to meet the goals.
        /// </summary>
        /// <returns>The daily suggested value.</returns>
        public float GetDailySuggestedValue(DayInfo today)
        {
            float result = 0;

            if (DateTime.Today > dueDate) return 0;

            if (this.Nature == NatureEnum.Standart)
            {
                if (GetRemainingDays(today) > 1) result = Math.Sign(goalValue - startingValue) * (goalValue - presentValue) / GetRemainingDays(today);
                else result = Math.Sign(goalValue - startingValue) * goalValue - presentValue;
            }
            if (this.Nature == NatureEnum.BetweenLimits)
            {
                if (presentValue < goalValue)
                {
                    if (GetRemainingDays(today) > 1) result = (goalValue - presentValue) / GetRemainingDays(today);
                    else result = goalValue - presentValue;
                }
                else result = 0;
            }

            if (result < 0) result = 0;
            return result;
        }
        /// <summary>
        /// Gets the present performence of goal completion.
        /// 100 * PresentPercentage / DesiredPresentPercentage
        /// </summary>
        /// <returns>The percentage</returns>
        public float GetPerformance(bool isFull, DayInfo today)
        {
            float result = 0;
            float presentPercentage = GetPresentPercentage();
            float desiredPercentage = GetDesiredPercentage(today);

            if (desiredPercentage > 0) result = 100 * presentPercentage / desiredPercentage;
            else
            {
                if (presentPercentage > 0) result = 100;
                else result = 0;
            }

            if (!isFull)
            {
                if (result > 100) result = 100;
            }
            if (result < 0) result = 0;

            return result;
        }
        /// <summary>
        /// Gets the relative performence of goal completion.
        /// 100 * (PresentPercentage - DesiredPercentage) / PresentPercentage)
        /// </summary>
        /// <returns>The percentage</returns>
        public float GetRelativePerformance(DayInfo today)
        {
            float result = 0;
            float factor1, factor2 = 0;
            float presentPercentage = GetPresentPercentage();
            float desiredPercentage = GetDesiredPercentage(today);

            if (presentPercentage > 0 && presentPercentage > desiredPercentage)
                factor1 = (100 * (presentPercentage - desiredPercentage) / presentPercentage) / 2;
            else factor1 = 0;

            factor2 = presentPercentage / 2;

            result = factor1 + factor2;

            if (result > 100) result = 100;
            if (result < 0) result = 0;

            return result;
        }
        public float GetPresentValue()
        {
            return presentValue;
        }
        public void SetPresentValue(float value)
        {
            presentValue = value;
        }
        #endregion

        #region Public Properties
        public long ID { get { return id; } set { id = value; }}
        public int GroupID { get { return groupID; } set { groupID = value; }}
        public string Definition { get { return definition; } set { definition = value; }}
        public string Details { get { return details; } set { details = value; }}
        public DateTime StartDate { get { return startDate; } set { startDate = value; }}
        public DateTime EndDate { get { return endDate; } set { endDate = value; }}
        public DateTime DueDate { get { return dueDate; } set { dueDate = value; }}
        public NatureEnum Nature { get { return nature; } set { nature = value; }}
        public DTC.RangeEnum Range { get { return range; } set { range = value; }}
        public int OwnerID { get { return ownerID; } set { ownerID = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }}
        public DTC.SizeEnum Size { get { return size; } set { size = value; }}
        public GoalTypeInfo.TypeEnum GoalType { get { return goalType; } set { goalType = value; }}
        public float StartingValue { get { return startingValue; } set { startingValue = value; }}
        public float GoalValue { get { return goalValue; } set { goalValue = value; }}
        public float GenericValue { get { return genericValue; } set { genericValue = value; }}
        public int PrimaryProjectID { get { return primaryProjectID; } set { primaryProjectID = value; }}
        public float EstimatedHours { get { return estimatedHours; } set { estimatedHours = value; }}
        public int EstimatedMinutes { get { return estimatedMinutes; } set { estimatedMinutes = value; }}
        public bool IsFocus { get { return isFocus; } set { isFocus = value; }}
        public int TemplateID { get { return templateID; } set { templateID = value; }}
        public float ThresholdValue { get { return thresholdValue; } set { thresholdValue = value; }}
        public int ItemID { get { return itemID; } set { itemID = value; }}
        public int ProjectGroupID { get { return projectGroupID; } set { projectGroupID = value; }}
        public int ProjectID { get { return projectID; } set { projectID = value; }}
        public int SecondaryProjectGroupID { get { return secondaryProjectGroupID; } set { secondaryProjectGroupID = value; }}
        public int SecondaryProjectID { get { return secondaryProjectID; } set { secondaryProjectID = value; }}
        public int TertiaryProjectGroupID { get { return tertiaryProjectGroupID; } set { tertiaryProjectGroupID = value; }}
        public int TertiaryProjectID { get { return tertiaryProjectID; } set { tertiaryProjectID = value; }}
        public string Hour { get { return hour; } set { hour = value; }}
        public int Criteria { get { return criteria; } set { criteria = value; }}
        public int MeasurementStyle { get { return measurementStyle; } set { measurementStyle = value; }}
        public int ItemNature { get { return itemNature; } set { itemNature = value; }}
        public int PragmaID { get { return pragmaID; } set { pragmaID = value; }}
        public int PragmaAttributeID { get { return pragmaAttributeID; } set { pragmaAttributeID = value; }}
        public string PragmaAttributePhrase { get { return pragmaAttributePhrase; } set { pragmaAttributePhrase = value; }}
        public float PragmaAttributeValue { get { return pragmaAttributeValue; } set { pragmaAttributeValue = value; }}
        public int PragmaNumInstances { get { return pragmaNumInstances; } set { pragmaNumInstances = value; }}
        public bool IsBlackAndWhite { get { return isBlackAndWhite; } set { isBlackAndWhite = value; }}
        public float PresentPercentage { get { return GetPresentPercentage(); } }
        public float PresentValue { get { return presentValue; } }
        #endregion

        object ICloneable.Clone() { return this.MemberwiseClone(); }
    }
}