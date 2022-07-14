using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class GoalsController : ApiController
    {
        [HttpGet]
        [Route("api/Goals/{goalID}")]
        public GoalInfo Get(int goalID)
        {
            return DB.Goals.GetGoal(goalID, true);
        }

        [HttpGet]
        [Route("api/Goals/{ownerID}/{param1}/{param2}/{param3}")]
        public IEnumerable<GoalInfo> Get(long ownerID, string param1, string param2, string param3)
        {
            OwnerInfo owner = DB.Owner.GetOwner(DTC.RangeEnum.Week, DateTime.Today);
            List<GoalInfo> goals = DB.Goals.GetGoals(owner, true);
                                                                           
            goals = goals.OrderByDescending(i => i.PresentPercentage).ToList();

            return goals;
        }

        [HttpGet]
        [Route("api/Goals/{rangeID}/{getPresentValues}/{standartOrProjected}")]
        public IEnumerable<GoalInfo> Get(int rangeID, bool getPresentValues, int standartOrProjected)
        {
            List<GoalInfo> goals = new List<GoalInfo>();
            DTC.RangeEnum range = (DTC.RangeEnum)rangeID;

            OwnerInfo owner = DB.Owner.GetOwner(range, DateTime.Today);
            goals = DB.Goals.GetGoals(owner, true);
            DayInfo today = DB.Days.GetDay(DateTime.Today, true);

            foreach (GoalInfo goal in goals)
            {
                if (standartOrProjected == 1)
                {
                    goal.PresentPercentage = goal.GetPresentPercentage();
                    goal.DesiredValue = goal.GoalValue;
                }
                if (standartOrProjected == 2)
                {
                    goal.PresentPercentage = goal.GetPerformance(false, today);     // What does it mean? isFull
                    goal.DesiredValue = goal.GetDesiredValue(today);
                }
                    
            }

            goals = goals.OrderByDescending(i=>i.Status).OrderByDescending(i => i.PresentPercentage).ToList();

            if(getPresentValues)
            {
                List<GoalGroupInfo> goalGroups = DB.Goals.GetGoalGroups();
                GoalsEngine goalEngine = new GoalsEngine(goals, goalGroups, today);
                GoalsEngine.PerformanceNatureEnum nature = GoalsEngine.PerformanceNatureEnum.Normal;
                if (standartOrProjected == 1)
                    nature = GoalsEngine.PerformanceNatureEnum.Worst;
                else if (standartOrProjected == 1)
                    nature = GoalsEngine.PerformanceNatureEnum.Normal;

                foreach (GoalInfo goal in goals)
                {
                    goal.Contribution = goalEngine.GetGoalContributionWeighted(goal, false, nature);
                    goal.ContributionMax = goalEngine.GetGoalContributionWeighted(goal, true, nature);
                }
            }


            return goals;
        }

        [HttpGet]
        [Route("api/Goals/{rangeID}/{projectParameter}")]
        public IEnumerable<ProjectInfo> Get(int rangeID, string projectParameter)
        {
            List<ProjectInfo> projects = new List<ProjectInfo>();
            DTC.RangeEnum range = (DTC.RangeEnum)rangeID;
            OwnerInfo owner = DB.Owner.GetOwner(range, DateTime.Today);
            List<GoalInfo> goals = DB.Goals.GetGoals(owner, true);
            DayInfo today = DB.Days.GetDay(DateTime.Today, true);
            
            foreach (GoalInfo goal in goals)
            {
                if (goal.PrimaryProjectID > 0)
                {
                    ProjectInfo project = DB.Projects.GetProject(goal.PrimaryProjectID);
                    ProjectGroupInfo projectGroup = DB.ProjectGroups.GetProjectGroup(project.ProjectGroupID);
                    project.SmartCode = project.GetSmartCode(projectGroup, false);
                    projects.Add(project);
                }
            }

            return projects;
        }

        [HttpPost]
        [Route("api/Goals/")]
        public void Post([FromBody]GoalInfo value)
        {
            DB.Goals.AddGoal(value);
        }

        [HttpPut]
        [Route("api/Goals/{goalID}")]
        public void Put(int goalID, [FromBody]string value)
        {

        }
        
        [HttpPut]
        [Route("api/Goals/{goalID}/{actionID}/{isCompleted}")]
        public void Put(int goalID, int actionID, bool isCompleted)
        {
            if(actionID == 1)
            {
                DB.Goals.UpdateGoalAsCompleted(goalID);
            }
            else if (actionID == 2)
            {
                DB.Goals.PostponeTotheNextDay(goalID);
            }
        }

        [HttpDelete]
        [Route("api/Goals/{goalID}")]
        public void Delete(int goalID)
        {
        }
    }
}
