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
        
        #region Members
        public long ID { get; set; }
        public int GroupID { get; set; }
        public string Definition { get; set; }
        public string Details { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DueDate { get; set; }
        public NatureEnum Nature { get; set; }
        public DTC.RangeEnum Range { get; set; }
        public int OwnerID { get; set; }
        public DTC.StatusEnum Status { get; set; }
        public DTC.SizeEnum Size { get; set; }
        public GoalTypeInfo.TypeEnum GoalType { get; set; }
        public float StartingValue { get; set; }
        public float GoalValue { get; set; }
        public float GenericValue { get; set; }
        public int PrimaryProjectID { get; set; }
        public float EstimatedHours { get; set; }
        public int EstimatedMinutes { get; set; }
        public bool IsFocus { get; set; }
        public int TemplateID { get; set; }
        public float ThresholdValue { get; set; }
        public long ItemID { get; set; }
        public int ProjectGroupID { get; set; }
        public int ProjectID { get; set; }
        public int SecondaryProjectGroupID { get; set; }
        public int SecondaryProjectID { get; set; }
        public int TertiaryProjectGroupID { get; set; }
        public int TertiaryProjectID { get; set; }
        public string Hour { get; set; }
        public int Criteria { get; set; }
        public int MeasurementStyle { get; set; }
        public int ItemNature { get; set; }
        public int PragmaID { get; set; }
        public int PragmaAttributeID { get; set; }
        public string PragmaAttributePhrase { get; set; }
        public float PragmaAttributeValue { get; set; }
        public int PragmaNumInstances { get; set; }
        public bool IsBlackAndWhite { get; set; }
        public float PresentPercentage { get; set; }
        public float DesiredValue { get; set; }
        public float PresentValue { get; private set; }
        public float Contribution { get; set; }
        public float ContributionMax { get; set; }
        public float HoursPerUnit { get; set; }
        public bool IsAchieved { get; private set; }
        #endregion
        
        #region Constructors
        public GoalInfo()
        {
            ID = 0;
            GroupID = 1;
            IsFocus = false;
            TemplateID = 0;
            Definition = string.Empty;
            Details = string.Empty;
            Nature = NatureEnum.Standart;
            Range = DTC.RangeEnum.Floating;
            OwnerID = 0;
            Status = DTC.StatusEnum.Running;
            Size = DTC.SizeEnum.Medium;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            DueDate = DateTime.Now;
            GoalType = GoalTypeInfo.TypeEnum.NA;
            PrimaryProjectID = 0;
            EstimatedMinutes = 0;
            EstimatedHours = 0;
            ThresholdValue = 0;
            ItemID = 0;
            ProjectGroupID = 0;
            ThresholdValue = 0;
            GoalValue = 0;
            GenericValue = 0;
            ProjectID = 0;
            SecondaryProjectGroupID = 0;
            SecondaryProjectID = 0;
            TertiaryProjectGroupID = 0;
            TertiaryProjectID = 0;
            Hour = "00:00";
            Criteria = 0;
            MeasurementStyle = 0;
            ItemNature = 0;
            PragmaID = 0;
            PragmaAttributeID = 0;
            PragmaAttributePhrase = string.Empty;
            PragmaAttributeValue = 0;
            PragmaNumInstances = 0;
            IsBlackAndWhite = false;
            Contribution = ContributionMax = 0;
            HoursPerUnit = 0;
            IsAchieved = false;
        }
        #endregion

        #region Private Methods
        private float GetPassedDays(DayInfo today)
        {
            float result = 0;

            TimeSpan upToNow = DateTime.Today.Subtract(StartDate);
            result = (float)upToNow.Days;

            result += today.GetTodaysPassedHours() / today.GetTodaysTotalHours();

            if (result < 0) result = 0;
            if (result > GetTotalDays()) result = GetTotalDays();

            return result;
        }
        private float GetTotalDays()
        {
            float result = 0;

            TimeSpan allSpan = DueDate.Subtract(StartDate);
            result = (float)allSpan.TotalDays + 1;
            if (result < 0) result = 0;

            return result;
        }
        #endregion

        #region Public Methods
        public bool IsNull() { return ID > 0 ; }
        public float GetDesiredPercentage(DayInfo today)
        {
            float result = 0;

            if (GetTotalDays() != 0)
            {
                if (Nature == NatureEnum.Standart)
                    result = 100 * GetPassedDays(today) / GetTotalDays();
                else if (Nature == NatureEnum.BetweenLimits)
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

            TimeSpan leftDays = DueDate.Subtract(DateTime.Today);
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

            if (GoalValue > 0)
            {
                if (Nature == NatureEnum.Standart)
                    result = StartingValue - GetDesiredPercentage(today) * (StartingValue - GoalValue) / 100;
                else if (Nature == NatureEnum.BetweenLimits)
                    result = StartingValue - (100 - GetDesiredPercentage(today)) * (StartingValue - GoalValue) / 100;
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
             
            if(Status == DTC.StatusEnum.Success)
            {
                result = 100;
            }
            else
            {
                if (GoalValue > 0)
                {
                    if(GoalType == GoalTypeInfo.TypeEnum.Weight && PresentValue == 0)
                    {
                        result = 0;
                    }
                    else
                    {
                        if (Nature == NatureEnum.Standart)
                            result = 100 * (PresentValue - StartingValue) / (GoalValue - StartingValue);
                        else if (Nature == NatureEnum.BetweenLimits)
                            result = 100 * (GoalValue - PresentValue) / GoalValue;
                    }
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

            if (DateTime.Today > DueDate) return 0;

            if (this.Nature == NatureEnum.Standart)
            {
                if (GetRemainingDays(today) > 1) result = Math.Sign(GoalValue - StartingValue) * (GoalValue - PresentValue) / GetRemainingDays(today);
                else result = Math.Sign(GoalValue - StartingValue) * GoalValue - PresentValue;
            }
            if (this.Nature == NatureEnum.BetweenLimits)
            {
                if (PresentValue < GoalValue)
                {
                    if (GetRemainingDays(today) > 1) result = (GoalValue - PresentValue) / GetRemainingDays(today);
                    else result = GoalValue - PresentValue;
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
            return PresentValue;
        }
        public void SetPresentValue(float value)
        {
            PresentValue = value;
            IsAchieved = false;

            if(Status == DTC.StatusEnum.Running)
            {
                if (Nature == NatureEnum.Standart && GoalType != GoalTypeInfo.TypeEnum.Weight)
                {
                    if (PresentValue >= GoalValue)
                        IsAchieved = true;
                }
            }
            
        }
        #endregion

        object ICloneable.Clone() { return this.MemberwiseClone(); }
    }
}