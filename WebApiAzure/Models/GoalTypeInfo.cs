using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class GoalTypeInfo
    {
        #region Enums
        public enum TypeEnum : int
        {
            NA = 0, TotalHours = 1, TotalLeverage = 2, NumberOfIdeas = 3, NumberOfThings = 4,
            Weight = 5, NumberOfProjectInstances = 8, TotalProjectHours = 9,
            NumberOfBooks = 10, Book = 12, Verbal = 13, Efficiency = 15, Segment = 16,
            ProjectGoal = 19, GenericNumeric = 21, Block = 24, WakeUp = 25, WakeUpExp = 31,
            Todo = 28, NumberOfTodos = 29,
            NumDaysOverPoint = 32, NumWeeksOverPoint = 33, NumMonthsOverPoint = 35,
            Pragma = 36,
            NumberOfSegments = 37, NumberOfBlocks = 38
        }
        public enum NatureEnum : int { Positive = 1, Negative = -1, BothPossible = 0 }
        #endregion

        #region  Members
        public int ID { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string EvaluationSQL { get; set; }
        public NatureEnum Nature { get; set; }
        public bool IsObjective { get; set; }
        public TypeEnum Type { get; set; }
        #endregion

        #region Constructors
        public GoalTypeInfo()
        {
            Code = "";
            Name = "";
            Nature = NatureEnum.BothPossible;
            Order = 0;
            EvaluationSQL = "";
            IsObjective = false;
            Type = TypeEnum.NA;
        }
        public GoalTypeInfo(int id, string code, string name)
        {
            ID = id;
            Code = code;
            Name = name;
        }
        #endregion
    }
}