using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using WebApiAzure.Models;

namespace WebApiAzure
{
    public class DB
    {
        public static string GetConnStr()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["Connection String MSSQL"].ConnectionString;
        }
        public static DataTable RunExecuteReaderMSSQL(string strSQL)
        {
            SqlConnection conn = new SqlConnection(GetConnStr());
            conn.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = strSQL;


            SqlDataReader reader = cmd.ExecuteReader();
            System.Data.DataTable dt = new System.Data.DataTable();

            while (!reader.IsClosed)
            {
                // DataTable.Load automatically advances the reader to the next result set
                dt.Load(reader);
            }

            reader.Close();
            conn.Close();

            return dt;
        }

        public static List<ProjectInfo> GetProjects()
        {
            List<ProjectInfo> data = new List<ProjectInfo>();

            string strSQL = "SELECT * FROM Projects";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
            {
                ProjectInfo project = new ProjectInfo(Convert.ToInt32(dr["ProjectID"]), Convert.ToString(dr["ProjectCode"]), Convert.ToString(dr["ProjectName"]));
                data.Add(project);
            }
            return data;
        }
        public static List<BlockInfo> GetBlocks(int projectID)
        {
            List<BlockInfo> data = new List<BlockInfo>();

            string strSQL = "SELECT * FROM Blocks " +
                " WHERE ProjectID = " + projectID;
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
            {
                BlockInfo block = new BlockInfo(Convert.ToInt32(dr["BlockID"]), Convert.ToString(dr["Title"]));

                block.Details = Convert.ToString(dr["Details"]);
                block.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                block.StartDate = Convert.ToDateTime(dr["StartDate"]);
                block.EndDate = Convert.ToDateTime(dr["EndDate"]);
                block.DueDate = Convert.ToDateTime(dr["DueDate"]);
                block.HasDue = Convert.ToBoolean(dr["HasDue"]);
                block.Status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);

                data.Add(block);
            }
            return data;
        }
        public static List<ProjectInfo> GetActionableProjects()
        {
            List<ProjectInfo> data = new List<Models.ProjectInfo>();

            string strSQL = "SELECT * FROM Projects WHERE IsActionable = 1 AND StatusID = 1";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
            {
                ProjectInfo project = new ProjectInfo(Convert.ToInt32(dr["ProjectID"]), Convert.ToString(dr["ProjectCode"]), Convert.ToString(dr["ProjectName"]));
                data.Add(project);
            }
            return data;
        }
        public static List<ProjectSnapshotInfo> GetProjectsSnapshot()
        {
            List<ProjectSnapshotInfo> data = new List<ProjectSnapshotInfo>();
            DateTime dtTreshold = DateTime.Now.AddDays(-30);

            string strSQL = "SELECT Projects.ProjectID, Projects.ProjectName, Projects.ProjectCode, SUM(Tasks.RealTime) AS RealTimeTotal, " +
                " ProjectGroups.ProjectGroupCode, Projects.DueDate, Projects.StartDate, ProjectImgName " +
                " FROM Projects " +
                " INNER JOIN Tasks ON Projects.ProjectID = Tasks.ProjectID " +
                " INNER JOIN ProjectGroups ON Projects.ProjectGroupID = ProjectGroups.ProjectGroupID " +
                " WHERE Projects.IsActionable = 1 " +
                " AND Projects.StatusID = 1 " +
                " AND Tasks.TaskDate >= " + DTC.ObtainGoodDT(dtTreshold, false) +
                " GROUP BY Projects.ProjectID, Projects.ProjectName, Projects.ProjectCode, " +
                " ProjectGroups.ProjectGroupCode, Projects.DueDate, Projects.StartDate, ProjectImgName " +
                " ORDER BY RealTimeTotal DESC";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
            {
                DateTime dtStartDate = Convert.ToDateTime(dr["StartDate"]);
                DateTime dtDueDate = Convert.ToDateTime(dr["DueDate"]);

                ProjectSnapshotInfo projectSnapshot = new ProjectSnapshotInfo();
                projectSnapshot.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                projectSnapshot.ProjectCode = Convert.ToString(dr["ProjectCode"]);
                projectSnapshot.ProjectName = Convert.ToString(dr["ProjectName"]);
                projectSnapshot.ProjectGroupCode = Convert.ToString(dr["ProjectGroupCode"]);
                projectSnapshot.RealTime = Convert.ToSingle(dr["RealTimeTotal"]);
                projectSnapshot.DueDate = DTC.GetSmartDateTime(dtDueDate, false);
                projectSnapshot.StartDate = DTC.GetSmartDateTime(dtStartDate, false);
                projectSnapshot.ProjectImgName = Convert.ToString(dr["ProjectImgName"]);
                projectSnapshot.PercentCompleted = 67;

                projectSnapshot.NumDaysRemaining = 0;
                if (dtDueDate > DateTime.Today)
                    projectSnapshot.NumDaysRemaining = (int)dtDueDate.Subtract(DateTime.Today).TotalDays;

                data.Add(projectSnapshot);
            }

            List<WeekInfo> weeks = new List<WebApiAzure.WeekInfo>();
            weeks.Add(new WeekInfo(DateTime.Today));
            weeks.Add(DTC.GetPreviousWeek(weeks[0]));
            weeks.Add(DTC.GetPreviousWeek(weeks[1]));
            weeks.Add(DTC.GetPreviousWeek(weeks[2]));

            for (int w=0;w<weeks.Count;w++)
            {
                strSQL = "SELECT Projects.ProjectID, SUM(Tasks.RealTime) AS RealTimeTotal" +
                " FROM Projects " +
                " INNER JOIN Tasks ON Projects.ProjectID = Tasks.ProjectID" +
                " WHERE Projects.IsActionable = 1  AND Projects.StatusID = 1" +
                " AND Tasks.TaskDate >= " + DTC.ObtainGoodDT(weeks[w].StartDate, true) +
                " AND Tasks.TaskDate <= " + DTC.ObtainGoodDT(weeks[w].EndDate, true) +
                " GROUP BY Projects.ProjectID";
                dt = RunExecuteReaderMSSQL(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    long projectID = Convert.ToInt32(dr["ProjectID"]);
                    float realTime = Convert.ToSingle(dr["RealTimeTotal"]);

                    if (data.Exists(i => i.ProjectID == projectID))
                    {
                        ProjectSnapshotInfo ps = data.Find(i => i.ProjectID == projectID);

                        if (w == 0) ps.W0 = realTime;
                        else if (w == 1) ps.W1 = realTime;
                        else if (w == 2) ps.W2 = realTime;
                        else if (w == 3) ps.W3 = realTime;
                    }
                }
            }

            List<long> projectIDs = new List<long>();
            foreach(ProjectInfo project in GetActionableProjects())
            {
                projectIDs.Add(project.ID);
                if(!data.Exists(i=>i.ProjectID == project.ID))
                {
                    ProjectSnapshotInfo ps = DB.GetProjectSnapshot(project.ID);
                    ps.RealTime = 0;
                    data.Add(ps);
                }
            }

            Dictionary<long, int> projectPerformances = GetProjectPerformances(projectIDs);
            foreach (int projectID in projectPerformances.Keys)
            {
                ProjectSnapshotInfo ps = data.Find(i=>i.ProjectID == projectID);
                if(ps != null)
                    ps.PercentCompleted = projectPerformances[projectID];
            }

            Dictionary<long, float> projectEternalTotalTimes = GetProjectEternalTotalTimes(projectIDs);
            foreach (int projectID in projectEternalTotalTimes.Keys)
            {
                ProjectSnapshotInfo ps = data.Find(i => i.ProjectID == projectID);
                if (ps != null)
                    ps.EternalTotalTime = projectEternalTotalTimes[projectID];
            }

            return data;
        }
        public static ProjectSnapshotInfo GetProjectSnapshot(int projectID)
        {
            ProjectSnapshotInfo projectSnapshot = new ProjectSnapshotInfo();

            string strSQL = "SELECT Projects.ProjectID, Projects.ProjectName, Projects.ProjectCode, SUM(Tasks.RealTime) AS RealTimeTotal, " +
                " ProjectGroups.ProjectGroupCode, Projects.DueDate, Projects.StartDate, ProjectImgName " +
                " FROM Projects " +
                " INNER JOIN Tasks ON Projects.ProjectID = Tasks.ProjectID " +
                " INNER JOIN ProjectGroups ON Projects.ProjectGroupID = ProjectGroups.ProjectGroupID " +
                " WHERE Projects.ProjectID = " + projectID +
                " GROUP BY Projects.ProjectID, Projects.ProjectName, Projects.ProjectCode, " +
                " ProjectGroups.ProjectGroupCode, Projects.DueDate, Projects.StartDate, ProjectImgName ";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            if (dt.Rows.Count == 1)
            {
                DataRow dr = dt.Rows[0];
                DateTime dtDueDate = Convert.ToDateTime(dr["DueDate"]);
                DateTime dtStartDate = Convert.ToDateTime(dr["StartDate"]);

                projectSnapshot.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                projectSnapshot.ProjectCode = Convert.ToString(dr["ProjectCode"]);
                projectSnapshot.ProjectName = Convert.ToString(dr["ProjectName"]);
                projectSnapshot.ProjectGroupCode = Convert.ToString(dr["ProjectGroupCode"]);
                projectSnapshot.RealTime = Convert.ToSingle(dr["RealTimeTotal"]);
                projectSnapshot.StartDate = DTC.GetSmartDateTime(dtStartDate, false);
                projectSnapshot.DueDate = DTC.GetSmartDateTime(dtDueDate, false);
                projectSnapshot.ProjectImgName = Convert.ToString(dr["ProjectImgName"]);
                projectSnapshot.PercentCompleted = GetProjectPerformance(projectID);

                projectSnapshot.NumDaysRemaining = 0;
                if (dtDueDate > DateTime.Today)
                    projectSnapshot.NumDaysRemaining = (int)dtDueDate.Subtract(DateTime.Today).TotalDays;
            }

            List<WeekInfo> weeks = new List<WebApiAzure.WeekInfo>();
            weeks.Add(new WeekInfo(DateTime.Today));
            weeks.Add(DTC.GetPreviousWeek(weeks[0]));
            weeks.Add(DTC.GetPreviousWeek(weeks[1]));
            weeks.Add(DTC.GetPreviousWeek(weeks[2]));

            for (int w = 0; w < weeks.Count; w++)
            {
                strSQL = "SELECT SUM(Tasks.RealTime) AS RealTimeTotal" +
                " FROM Projects " +
                " INNER JOIN Tasks ON Projects.ProjectID = Tasks.ProjectID" +
                " WHERE Projects.ProjectID = " + projectID +
                " AND Tasks.TaskDate >= " + DTC.ObtainGoodDT(weeks[w].StartDate, true) +
                " AND Tasks.TaskDate <= " + DTC.ObtainGoodDT(weeks[w].EndDate, true);
                dt = RunExecuteReaderMSSQL(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    float realTime = 0;
                    if(DTC.IsNumeric(dr["RealTimeTotal"]))
                        realTime = Convert.ToSingle(dr["RealTimeTotal"]);

                    if (w == 0) projectSnapshot.W0 = realTime;
                    else if (w == 1) projectSnapshot.W1 = realTime;
                    else if (w == 2) projectSnapshot.W2 = realTime;
                    else if (w == 3) projectSnapshot.W3 = realTime;
                }
            }

            projectSnapshot.EternalTotalTime = GetProjectEternalTotalTimes(projectID);

            return projectSnapshot;
        }

        public static int GetProjectPerformance(int projectID)
        {
            int result = 0;
            string strSQL= "SELECT SUM(Segments.Size) AS TotalSize, Segments.StatusID " +
                " FROM Segments " +
                " INNER JOIN Blocks ON Segments.BlockID = Blocks.BlockID " +
                " WHERE Blocks.ProjectID = " + projectID +
                " GROUP BY Segments.StatusID";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            float sizeTotal = 0;
            float sizeCompleted = 0;

            foreach (DataRow dr in dt.Rows)
            {
                sizeTotal += Convert.ToSingle(dr["TotalSize"]);
                DTC.StatusEnum status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);
                if(status!= DTC.StatusEnum.Running)
                    sizeCompleted+= Convert.ToSingle(dr["TotalSize"]);
            }

            if (sizeTotal > 0)
                result = (int)Math.Round(100f * sizeCompleted / sizeTotal);

            return result;
        }
        public static Dictionary<long, int> GetProjectPerformances(List<long> projectIDs)
        {
            Dictionary<long, int> data = new Dictionary<long, int>();

            string strSQLProjectIDs = "WHERE Blocks.ProjectID = 0 ";
            foreach (int projectID in projectIDs)
                strSQLProjectIDs += " OR Blocks.ProjectID = " + projectID;

            string strSQL = "SELECT Blocks.ProjectID, Segments.StatusID, SUM(Segments.Size) AS TotalSize " +
                " FROM Segments  INNER JOIN Blocks ON Segments.BlockID = Blocks.BlockID " +
                strSQLProjectIDs +
                " GROUP BY Blocks.ProjectID, Segments.StatusID " +
                " ORDER BY Blocks.ProjectID, Segments.StatusID";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (int projectID in projectIDs)
            {
                float sizeTotal = 0;
                float sizeCompleted = 0;
                int percentage = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    if(Convert.ToInt32(dr["ProjectID"]) == projectID)
                    {
                        sizeTotal += Convert.ToSingle(dr["TotalSize"]);
                        DTC.StatusEnum status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);
                        if (status != DTC.StatusEnum.Running)
                            sizeCompleted += Convert.ToSingle(dr["TotalSize"]);
                    }

                    if (sizeTotal > 0)
                        percentage = (int)Math.Round(100f * sizeCompleted / sizeTotal);
                }

                data.Add(projectID, percentage);
            }

            return data;
        }
        public static Dictionary<long, float> GetProjectEternalTotalTimes(List<long> projectIDs)
        {
            Dictionary<long, float> data = new Dictionary<long, float>();
            
            string strSQL = "SELECT SUM(Tasks.RealTime) AS EternalTotalTime, Tasks.ProjectID " +
                " FROM Tasks " +
                " INNER JOIN Projects ON Tasks.ProjectID = Projects.ProjectID " +
                " WHERE Projects.IsActionable = 1 AND Projects.StatusID = 1 " +
                " GROUP BY Tasks.ProjectID";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (long projectID in projectIDs)
                data.Add(projectID, 0f);

            foreach (DataRow dr in dt.Rows)
            {
                long projectID = Convert.ToInt32(dr["ProjectID"]);

                if (data.ContainsKey(projectID))
                    data[projectID] = Convert.ToSingle(dr["EternalTotalTime"]);
            }

            return data;
        }
        public static float GetProjectEternalTotalTimes(long projectID)
        {
            float result = 0;

            List<long> list = new List<long>();
            list.Add(projectID);

            Dictionary<long, float> data = GetProjectEternalTotalTimes(list);
            if (data.ContainsKey(projectID))
                result = data[projectID];

            return result;
        }

        public static List<SegmentInfo> GetSegments(long blockID)
        {
            List<SegmentInfo> data = new List<SegmentInfo>();

            string strSQL = "SELECT * " +
                " FROM Segments " +
                " WHERE BlockID = " + blockID;

            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
            {
                SegmentInfo block = new SegmentInfo(Convert.ToInt32(dr["SegmentID"]), Convert.ToString(dr["Title"]));

                block.Details = Convert.ToString(dr["Details"]);
                block.BlockID = Convert.ToInt32(dr["BlockID"]);
                block.Status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);

                data.Add(block);
            }
            return data;
        }
    }
}