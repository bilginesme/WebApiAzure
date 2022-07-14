using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApiAzure.Models;

namespace WebApiAzure
{
    public class GoalsEngine
    {
        public enum PerformanceNatureEnum { Normal, Worst, Best }

        #region Private Members
        List<GoalInfo> goals;
        List<GoalGroupInfo> goalGroups;
        DayInfo today;
        #endregion

        #region Constructors
        public GoalsEngine(List<GoalInfo> goals, List<GoalGroupInfo> goalGroups, DayInfo today)
        {
            this.goals = goals;
            this.goalGroups = goalGroups;
            this.today = today;
        }
        #endregion

        /// <summary>
        /// Returns the (expected) performance of the goals in the dictionary.
        /// Output is [0,1]
        /// </summary>
        /// <param name="goals">A goals dictionary</param>
        /// <returns>Performance within range [0,1]</returns>
        public float GetPerformance(PerformanceNatureEnum perfNature)
        {
            float result = 0;

            foreach (GoalGroupInfo gg in goalGroups)
            {
                float weight = GetGroupWeight(gg.ID);
                float contribution = GetGroupContributionForAll(gg.ID, false, perfNature);
                float contrMax = GetGroupContributionForAll(gg.ID, true, perfNature);
                float grade = 0;
                if (contrMax > 0) grade = contribution / contrMax;

                result += grade * weight;
            }

            return result;
        }
        private float GetTotalSize()
        {
            float totalSize = 0;

            foreach (GoalInfo goal in goals)
                totalSize += (float)((int)goal.Size);

            return totalSize;
        }
        private float GetGoalContributionSimple(GoalInfo goal, PerformanceNatureEnum perfNature)
        {
            float okSize = 0;
            float divider = 0;

            if (perfNature == PerformanceNatureEnum.Worst) divider = 100;
            else divider = goal.GetDesiredPercentage(today);

            float presentValue = goal.GetPresentValue();
            float presentPercentage = goal.GetPresentPercentage();

            // we used to use pow2 in calculating the performance, this was a geometric penalty
            // for uncompleted goals, but it resulted a lack of motivation to start a task, so
            // we set it to 1
            // remove it after August 2010
            if (goal.Status == DTC.StatusEnum.Success)
            {
                okSize = (float)((int)goal.Size);
            }
            else if (goal.Status == DTC.StatusEnum.Running)
            {
                if (presentPercentage == 0) okSize = 0;
                else
                {
                    if (presentPercentage > divider)
                    {
                        okSize = (float)((int)goal.Size);
                    }
                    else
                    {
                        if (goal.IsBlackAndWhite)
                            okSize = 0;
                        else
                            okSize = (float)((int)goal.Size)
                                * (float)Math.Pow((double)(presentPercentage / divider), 1);
                    }
                }
            }
            else if (goal.Status == DTC.StatusEnum.Fail)
            {
                if (presentPercentage == 0) okSize = 0;
                else
                {
                    if (presentPercentage > divider)
                    {
                        okSize = (float)((int)goal.Size);
                    }
                    else
                    {
                        if (goal.IsBlackAndWhite)
                            okSize = 0;
                        else
                            okSize = (float)((int)goal.Size)
                                * (float)Math.Pow((double)(presentPercentage / divider), 1);
                    }
                }
            }

            return okSize;
        }
        public float GetGoalContributionForAll(GoalInfo goal, bool isMax, PerformanceNatureEnum perfNature)
        {
            float goalSize = 0;
            if (isMax)
                goalSize = (float)((int)goal.Size);
            else
                goalSize = GetGoalContributionSimple(goal, perfNature);

            float totalSize = GetTotalSize();

            if (totalSize > 0)
                return 100 * goalSize / totalSize;
            else
                return 0;
        }
        public float GetGroupContributionForAll(int groupID, bool isMax, PerformanceNatureEnum perfNature)
        {
            float groupSize = 0;

            foreach (GoalInfo g in goals)
            {
                if (g.GroupID == groupID)
                {
                    if (isMax)
                        groupSize += (float)((int)g.Size);
                    else
                        groupSize += GetGoalContributionSimple(g, perfNature);
                }
            }

            float totalSize = GetTotalSize();

            if (totalSize > 0)
                return 100 * groupSize / totalSize;
            else
                return 0;
        }
        /// <summary>
        /// This is the compact and sufficient method to get the overall contribution
        /// of a single goal, among a given "goals" dictionary. This method, also takes
        /// care of group weights, thus gives a complete contribution figure.
        /// </summary>
        /// <param name="goal">The goal of interest</param>
        /// <param name="goals">Goals dictionary. This method assumes that 'goals' dictionary contains 'goal' object.</param>
        /// <param name="isMax">True if you need the maximum contribution of the goal - if it's achieved</param>
        /// <returns>The contribution value of the goal.</returns>
        public float GetGoalContributionWeighted(GoalInfo goal, bool isMax, PerformanceNatureEnum performanceNature)
        {
            float nF = GetNormFactor(goal);

            if (isMax)
                return nF * GetGoalContributionForAll(goal, true, performanceNature);
            else
                return nF * GetGoalContributionForAll(goal, false, performanceNature);
        }
        /// <summary>
        /// This is the full complete method for getting a 'goal's grade contribution'.
        /// The grades of the goals within a GoalGroup sums up to 100,
        /// thus the grade contributions are given as integers.
        /// </summary>
        /// <param name="goal">The goal of interest</param>
        /// <param name="goals">The goals dictionary. The complete set of goals should be provided - not only the goals in the group.</param>
        /// <param name="isMax">The grade contribution of the goal</param>
        /// <returns></returns>
        public int GetGoalContributionGrade(GoalInfo goal, bool isMax, PerformanceNatureEnum performanceNature)
        {
            float nF = GetNormFactor(goal);
            float contribution = GetGoalContributionWeighted(goal, false, performanceNature);
            float contributionMax = GetGoalContributionWeighted(goal, true, performanceNature);
            float contrGroupMax = GetGroupContributionForAll(goal.GroupID, true, performanceNature);

            if (isMax)
                return (int)Math.Round(100 * contributionMax / (nF * contrGroupMax));
            else
                return (int)Math.Round(100 * contribution / (nF * contrGroupMax));
        }
        private float GetNormFactor(GoalInfo goal)
        {
            float weightGroup = GetGroupWeight(goal.GroupID);
            float contrGroupMax = GetGroupContributionForAll(goal.GroupID, true, PerformanceNatureEnum.Normal);

            float weightedMax = weightGroup * 100;
            float nF = weightedMax / contrGroupMax;     // norm factor

            return nF;
        }
        public float GetGroupWeight(int groupID)
        {
            float totalSize = 0;
            foreach (GoalGroupInfo gg in goalGroups)
                totalSize += Convert.ToSingle(gg.Leverage);

            float groupSize = 0;
            GoalGroupInfo goalGroup = goalGroups.Find(i=>i.ID == groupID);
            if(goalGroup != null)
                groupSize = Convert.ToSingle(goalGroup.Leverage);

            if (totalSize > 0) return groupSize / totalSize;
            else return 0;
        }
        public float GetMaxGroupWeight()
        {
            float maxWeight = 0;

            foreach (GoalGroupInfo gg in goalGroups)
            {
                float weight = GetGroupWeight(gg.ID);
                if (weight > maxWeight) maxWeight = weight;
            }

            return maxWeight;
        }
        public float GetMaxGoalContributionInGroup(int groupID)
        {
            float result = 0;

            foreach (GoalInfo goal in goals)
            {
                if (goal.GroupID == groupID)
                {
                    float c = GetGoalContributionForAll(goal, true, PerformanceNatureEnum.Normal);
                    if (c > result) result = c;
                }
            }

            return result;
        }
        public void CloneGoalToDB(GoalInfo goal, PeriodInfo newPeriod)
        {
            DB.Goals.AddGoal(CloneGoal(goal, newPeriod));
        }
        public GoalInfo CloneGoal(GoalInfo goal, PeriodInfo newPeriod)
        {
            GoalInfo cloneGoal = (GoalInfo)((ICloneable)goal).Clone();

            cloneGoal.StartDate = newPeriod.StartDate;
            cloneGoal.DueDate = newPeriod.EndDate;
            cloneGoal.EndDate = newPeriod.EndDate;
            cloneGoal.Definition = "CLONE --- " + goal.Definition;
            cloneGoal.Status = DTC.StatusEnum.Running;
            cloneGoal.OwnerID = DB.Owner.GetOwnerID(cloneGoal.Range, cloneGoal.StartDate);

            return cloneGoal;
        }
        public GoalInfo CloneGoal(GoalInfo goal)
        {
            PeriodInfo period = new PeriodInfo(goal.StartDate, goal.DueDate);
            return CloneGoal(goal, period);
        }
        public ProjectInfo GetProject(GoalInfo goal)
        {
            ProjectInfo project = new ProjectInfo();
            long projectID = 0;
            if (goal.GoalType == GoalTypeInfo.TypeEnum.ProjectGoal)
            {
                projectID = goal.ItemID;
            }
            if (goal.GoalType == GoalTypeInfo.TypeEnum.Block)
            {
                BlockInfo block = DB.Blocks.GetBlock(goal.ItemID);
                projectID = block.ProjectID;
            }
            else if (goal.GoalType == GoalTypeInfo.TypeEnum.Segment)
            {
                SegmentInfo segment = DB.Segments.GetSegment(goal.ItemID);
                BlockInfo block = DB.Blocks.GetBlock(segment.BlockID);
                projectID = block.ProjectID;
            }

            if (projectID > 0)
                project = DB.Projects.GetProject(projectID);
            return project;
        }
    }
}  