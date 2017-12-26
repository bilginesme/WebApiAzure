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
        [Route("api/Goals/{ownerID}")]
        public IEnumerable<GoalInfo> Get(long ownerID)
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
                    goal.PresentPercentage = goal.GetPresentPercentage();
                if (standartOrProjected == 2)
                    goal.PresentPercentage = goal.GetPerformance(false, today);     // What does it mean? isFull
            }

            goals = goals.OrderByDescending(i=>i.Status).OrderByDescending(i => i.PresentPercentage).ToList();

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

        // GET: api/Goals/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Goals
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Goals/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Goals/5
        public void Delete(int id)
        {
        }
    }
}
