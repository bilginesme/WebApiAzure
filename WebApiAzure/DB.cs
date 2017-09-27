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
        private static void RunNonQuery(string strSQL)
        {
            SqlConnection conn = new SqlConnection(GetConnStr());
            conn.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            conn.Close();
        }

        public static List<ProjectInfo> GetProjects()
        {
            List<ProjectInfo> data = new List<ProjectInfo>();

            string strSQL = "SELECT * FROM Projects";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
                data.Add(GetProject(dr));

            return data;
        }
        public static ProjectInfo GetProject(int projectID)
        {
            string strSQL = "SELECT * " +
            " FROM Projects " +
            " WHERE ProjectID = " + projectID;

            DataTable dt = RunExecuteReaderMSSQL(strSQL);
            if (dt.Rows.Count == 1)
                return GetProject(dt.Rows[0]);
            else
                return null;
        }
        private static ProjectInfo GetProject(DataRow dr)
        {
            ProjectInfo project = new ProjectInfo();

            project.ID = Convert.ToInt32(dr["ProjectID"]);
            project.Name = Convert.ToString(dr["ProjectName"]);
            project.Code = Convert.ToString(dr["ProjectCode"]);
            project.StartDate = Convert.ToDateTime(dr["StartDate"]);

            return project;
        }
        public static List<ProjectInfo> GetActionableProjects()
        {
            List<ProjectInfo> data = new List<Models.ProjectInfo>();

            string strSQL = "SELECT * FROM Projects WHERE IsActionable = 1 AND StatusID = 1";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
                data.Add(GetProject(dr));

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

            for (int w = 0; w < weeks.Count; w++)
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
            foreach (ProjectInfo project in GetActionableProjects())
            {
                projectIDs.Add(project.ID);
                if (!data.Exists(i => i.ProjectID == project.ID))
                {
                    ProjectSnapshotInfo ps = DB.GetProjectSnapshot(project.ID);
                    ps.RealTime = 0;
                    data.Add(ps);
                }
            }

            Dictionary<long, int> projectPerformances = GetProjectPerformances(projectIDs);
            foreach (int projectID in projectPerformances.Keys)
            {
                ProjectSnapshotInfo ps = data.Find(i => i.ProjectID == projectID);
                if (ps != null)
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
                    if (DTC.IsNumeric(dr["RealTimeTotal"]))
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
            string strSQL = "SELECT SUM(Segments.Size) AS TotalSize, Segments.StatusID " +
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
                if (status != DTC.StatusEnum.Running)
                    sizeCompleted += Convert.ToSingle(dr["TotalSize"]);
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
                    if (Convert.ToInt32(dr["ProjectID"]) == projectID)
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

        public static List<BlockInfo> GetBlocks(int projectID)
        {
            List<BlockInfo> data = new List<BlockInfo>();

            string strSQL = "SELECT * FROM Blocks " +
                " WHERE ProjectID = " + projectID;
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
                data.Add(GetBlock(dr));

            return data;
        }
        public static BlockInfo GetBlock(long blockID)
        {
            string strSQL = "SELECT * " +
            " FROM Blocks " +
            " WHERE BlockID = " + blockID;

            DataTable dt = RunExecuteReaderMSSQL(strSQL);
            if (dt.Rows.Count == 1)
                return GetBlock(dt.Rows[0]);
            else
                return null;
        }
        private static BlockInfo GetBlock(DataRow dr)
        {
            BlockInfo block = new BlockInfo(Convert.ToInt32(dr["BlockID"]), Convert.ToString(dr["Title"]));

            block.Details = Convert.ToString(dr["Details"]);
            block.ProjectID = Convert.ToInt32(dr["ProjectID"]);
            block.StartDate = Convert.ToDateTime(dr["StartDate"]);
            block.EndDate = Convert.ToDateTime(dr["EndDate"]);
            block.DueDate = Convert.ToDateTime(dr["DueDate"]);
            block.HasDue = Convert.ToBoolean(dr["HasDue"]);
            block.Status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);

            return block;
        }
        public static List<BlockEnhancedInfo> GetBlocksEnhanced(int projectID)
        {
            List<BlockInfo> blocks = GetBlocks(projectID);
            List<BlockEnhancedInfo> data = new List<BlockEnhancedInfo>();
            Dictionary<long, int> totalMinutesPerBlock = GetTotalMinutesPerBlock(projectID);

            string strSQL = "SELECT Blocks.BlockID, COUNT(Segments.SegmentID) AS TotalSegments, Segments.StatusID AS SegmentStatusID " +
                " FROM Blocks " +
                " INNER JOIN Segments ON Blocks.BlockID = Segments.BlockID " +
                " WHERE Blocks.ProjectID = " + projectID +
                " GROUP BY Blocks.StatusID, Blocks.Title, Blocks.BlockID, Segments.StatusID " +
                " ORDER BY Blocks.StatusID DESC, Blocks.Title";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach(BlockInfo block in blocks)
            {
                BlockEnhancedInfo be = new BlockEnhancedInfo();
                be.Details = block.Details;
                be.DueDate = block.DueDate;
                be.EndDate = block.EndDate;
                be.HasDue = block.HasDue;
                be.ID = block.ID;
                be.ProjectID = block.ProjectID;
                be.StartDate = block.StartDate;
                be.Status = block.Status;
                be.Title = block.Title;

                if (totalMinutesPerBlock.ContainsKey(be.ID))
                    be.TotalMinutes = totalMinutesPerBlock[be.ID];

                data.Add(be);

                foreach (DataRow dr in dt.Rows)
                {
                    long blockID = Convert.ToInt32(dr["BlockID"]);
                    if(blockID == block.ID)
                    {
                        BlockEnhancedInfo blockEnhanced = data.Find(i=>i.ID == block.ID);
                        if (blockEnhanced == null)
                        {
                            blockEnhanced = new BlockEnhancedInfo();
                            data.Add(blockEnhanced);
                        }

                        int segmentStatusID = Convert.ToInt16(dr["SegmentStatusID"]);
                        int totalSegments = Convert.ToInt16(dr["TotalSegments"]);
                     
                        blockEnhanced.NumSegmentsTotal += totalSegments;
                        if (segmentStatusID != (int)DTC.StatusEnum.Running)
                            blockEnhanced.NumSegmentsCompleted += totalSegments;
                    }
                }
            }

            data = data.OrderBy(i => i.Title).ToList();

            List<BlockEnhancedInfo> beList = new List<BlockEnhancedInfo>();
            beList.AddRange(data.FindAll(i=>i.Status == DTC.StatusEnum.Running));
            beList.AddRange(data.FindAll(i => i.Status == DTC.StatusEnum.Success));
            beList.AddRange(data.FindAll(i => i.Status == DTC.StatusEnum.Fail));

            return beList;
        }
        public static Dictionary<long, int> GetTotalMinutesPerBlock(int projectID)
        {
            Dictionary<long, int> data = new Dictionary<long, int>();

            string strSQL= "SELECT Blocks.BlockID, SUM(Tasks.RealTime) AS TotalMinutes " +
            " FROM Blocks " +
            " INNER JOIN Tasks ON Blocks.BlockID = Tasks.BlockID " +
            " WHERE Blocks.ProjectID = " + projectID +
            " GROUP BY Blocks.BlockID";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
                data.Add(Convert.ToInt32(dr["BlockID"]), Convert.ToInt16(dr["TotalMinutes"]));

            return data;
        }

        public static List<SegmentInfo> GetOldestSegments(long projectID)
        {
            List<SegmentInfo> segments = new List<SegmentInfo>();

            string strSQL = "SELECT Segments.* " +
                " FROM Blocks " +
                " INNER JOIN Segments ON Blocks.BlockID = Segments.BlockID " +
                " WHERE Blocks.ProjectID = " + projectID +
                " AND Segments.StatusID = " + (int)DTC.StatusEnum.Running +
                " ORDER BY Segments.StartDate";

            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
                segments.Add(GetSegment(dr));

            return segments;
        }
        public static List<SegmentInfo> GetSegments(long blockID)
        {
            List<SegmentInfo> segmentsRaw = new List<SegmentInfo>();
            List<SegmentInfo> segmentsOrdered = new List<SegmentInfo>();

            string strSQL = "SELECT * " +
                " FROM Segments " +
                " WHERE BlockID = " + blockID;

            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
                segmentsRaw.Add(GetSegment(dr));

            segmentsRaw = segmentsRaw.OrderBy(i => i.Title).ToList();
            segmentsOrdered.AddRange(segmentsRaw.FindAll(i => i.Status == DTC.StatusEnum.Running));
            segmentsOrdered.AddRange(segmentsRaw.FindAll(i => i.Status == DTC.StatusEnum.Success));
            segmentsOrdered.AddRange(segmentsRaw.FindAll(i => i.Status == DTC.StatusEnum.Fail));

            return segmentsOrdered;
        }
        public static SegmentInfo GetSegment(long segmentID)
        {
            string strSQL = "SELECT * " +
            " FROM Segments " +
            " WHERE SegmentID = " + segmentID;

            DataTable dt = RunExecuteReaderMSSQL(strSQL);
            if (dt.Rows.Count == 1)
                return GetSegment(dt.Rows[0]);
            else
                return null;
        }
        public static SegmentInfo GetSegment(DataRow dr)
        {
            SegmentInfo segment = new SegmentInfo();

            segment.ID = Convert.ToInt32(dr["SegmentID"]);
            segment.Title = Convert.ToString(dr["Title"]);
            segment.Details = Convert.ToString(dr["Details"]);
            segment.BlockID = Convert.ToInt32(dr["BlockID"]);
            segment.Status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);
            segment.StartDate = Convert.ToDateTime(dr["StartDate"]);

            return segment;
        }
        public static string UpdateSegment(SegmentInfo segment)
        {
            string strSQL = "UPDATE Segments SET " +
                " Title = '" + DTC.InputText(segment.Title, 255) + "'," +
                " BlockID = " + segment.BlockID + "," +
                " Details = '" + DTC.InputText(segment.Details, 255) + "'," +
                " StartDate = " + DTC.ObtainGoodDT(segment.StartDate, true) + "," +
                " StatusID = " + (int)segment.Status +
                " WHERE SegmentID = " + segment.ID;
            RunNonQuery(strSQL);

            return "OK";
        }
        public static string AddSegment(SegmentInfo segment)
        {
            string strSQL = "INSERT Segments (BlockID, Title, Details, StartDate, EndDate, DueDate, HasDue, Size, StatusID) VALUES (" +
                segment.BlockID + "," +
                "'" + DTC.InputText(segment.Title, 255) + "'," +
                "'" + DTC.InputText(segment.Details, 255) + "'," +      
                DTC.ObtainGoodDT(segment.StartDate, true) + "," +
                DTC.ObtainGoodDT(DateTime.Today, true) + "," +
                DTC.ObtainGoodDT(DateTime.Today, true) + "," +
                0 + "," +
                2 + "," +
                (int)DTC.StatusEnum.Running + 
                ")";
            RunNonQuery(strSQL);

            return "OK";
        }
        public static void DeleteSegment(long segmentID)
        {
            string strSQL = "DELETE Segments WHERE SegmentID = " + segmentID;
            RunNonQuery(strSQL);
        }

        public static Dictionary<string, int> GetRealMinutesPerWeek(int projectID, int nTop)
        {
            Dictionary<string, int> data = new Dictionary<string, int>();

            string strSQL = "SELECT TOP (" + nTop + ") SUM(RealTime) AS TotalPerWeek, " +
                " CONVERT(varchar, { fn YEAR(TaskDate) }) + CONVERT(varchar, { fn WEEK(TaskDate) }) AS TheWeek " +
                " FROM Tasks " +
                " WHERE ProjectID = " + projectID +
                " GROUP BY CONVERT(varchar, { fn YEAR(TaskDate) }) +CONVERT(varchar, { fn WEEK(TaskDate) }) " +
                " ORDER BY TheWeek DESC";
            DataTable dt = RunExecuteReaderMSSQL(strSQL);

            foreach (DataRow dr in dt.Rows)
            {
                data.Add(Convert.ToString(dr["TheWeek"]), Convert.ToInt16(dr["TotalPerWeek"]));
            }

            return data;
        }
    }
}