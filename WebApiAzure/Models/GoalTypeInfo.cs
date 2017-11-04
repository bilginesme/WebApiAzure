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
        public enum NatureEnum : int { Positive = 1, Negative = -1, BothPossible = 0
        }
        #endregion

        #region Private Members
        int id;
        string code, name;
        int order;
        bool isObjective;
        NatureEnum nature;
        string evaluationSQL;
        TypeEnum type;
        #endregion

        #region Constructors
        public GoalTypeInfo()
        {
            code = "";
            name = "";
            nature = NatureEnum.BothPossible;
            order = 0;
            evaluationSQL = "";
            isObjective = false;
            type = TypeEnum.NA;
        }
        public GoalTypeInfo(int id, string code, string name)
        {
            this.id = id;
            this.code = code;
            this.name = name;
        }
        #endregion

        #region Public Properties
        public int ID { get { return id; } set { id = value; }}
        public int Order { get { return order; } set { order = value; }}
        public string Name { get { return name; } set { name = value; }}
        public string Code { get { return code; } set { code = value; }}
        public string EvaluationSQL { get { return evaluationSQL; } set { evaluationSQL = value; }}
        public NatureEnum Nature { get { return nature; } set { nature = value; }}
        public bool IsObjective { get { return isObjective; } set { isObjective = value; }}
        public TypeEnum Type { get { return type; } set { type = value; }}
        #endregion
    }
}