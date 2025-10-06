using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class GoalsSuggestionsController : ApiController
    {
        [HttpGet]
        [Route("api/GoalsSuggestions/{rangeID}/{numItems}/{isIncludeBetweenLimitGoals}/{standartOrProjected}")]
        public IEnumerable<GoalInfo> Get(int rangeID, int numItems, bool isIncludeBetweenLimitGoals, int standartOrProjected)
        {
            DTC.RangeEnum range = (DTC.RangeEnum)rangeID;
            OwnerInfo owner = DB.Owner.GetOwner(range, DateTime.Today);
            List<GoalInfo> goalsAll = DB.Goals.GetGoals(owner, true);
            List<GoalGroupInfo> goalGroups = DB.Goals.GetGoalGroups();

            List<GoalInfo> goalsPicked = new List<GoalInfo>();
            DayInfo today = DB.Days.GetDay(DateTime.Today, true);
            GoalsEngine goalEngine = new GoalsEngine(goalsAll, goalGroups, today);

            GoalsEngine.PerformanceNatureEnum perfornanceMode = GoalsEngine.PerformanceNatureEnum.Worst;
            if(standartOrProjected == 2)
            {
                perfornanceMode = GoalsEngine.PerformanceNatureEnum.Normal;
            }
                
            if (!isIncludeBetweenLimitGoals)
                goalsAll = goalsAll.FindAll(i=>i.Nature == GoalInfo.NatureEnum.Standart);

            foreach(GoalInfo goal in goalsAll)
            {
                goal.Contribution = goalEngine.GetGoalContributionWeighted(goal, false, perfornanceMode);
                goal.ContributionMax = goalEngine.GetGoalContributionWeighted(goal, true, GoalsEngine.PerformanceNatureEnum.Worst);
                goal.DesiredValue = goal.GetDesiredValue(today);
            }

            int nCount = 0;
            foreach (GoalInfo gOut in goalsAll)
            {
                float maxValue = 0;
                long maxID = 0;

                foreach (GoalInfo gIn in goalsAll)
                {
                    float contrDifference = gIn.ContributionMax - gIn.Contribution;

                    if (contrDifference > maxValue && !goalsPicked.Exists(i => i.ID == gIn.ID) && gIn.Status == DTC.StatusEnum.Running)
                    {
                        maxValue = contrDifference;
                        maxID = gIn.ID;
                    }
                }

                if (maxID > 0 && !goalsPicked.Exists(i => i.ID == maxID))
                    goalsPicked.Add(goalsAll.Find(i => i.ID == maxID));

                nCount++;
                if (nCount >= numItems)
                    break;
            }

            return goalsPicked;
        }
    }
}
