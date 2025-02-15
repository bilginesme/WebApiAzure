﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using WebApiAzure.Models;
using static WebApiAzure.DB;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebApiAzure
{
    public class DB
    {
        public enum AddOrUpdate
        {
            Add, Update
        }

        public static string GetConnStr()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["Connection String MSSQL"].ConnectionString;
        }
        public static DataTable RunExecuteReader(string strSQL)
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
        private static long RunExecuteScalar(string strSQL)
        {
            long result = 0;

            SqlConnection conn = new SqlConnection(GetConnStr());
            conn.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = strSQL;

            result = Convert.ToInt32(cmd.ExecuteScalar());

            conn.Close();

            return result;
        }

        public static List<BlockInfo> GetBlocks(int projectID)
        {
            List<BlockInfo> data = new List<BlockInfo>();

            string strSQL = "SELECT * FROM Blocks " +
                " WHERE ProjectID = " + projectID;
            DataTable dt = RunExecuteReader(strSQL);
            foreach (DataRow dr in dt.Rows)
                data.Add(GetBlock(dr));

            return data;
        }
        public static List<BlockInfo> GetBlocksOnlyRunning(int projectID)
        {
            List<BlockInfo> data = new List<BlockInfo>();

            string strSQL = "SELECT * FROM Blocks " +
                " WHERE ProjectID = " + projectID +
                " AND StatusID = 1";
            DataTable dt = RunExecuteReader(strSQL);
            foreach (DataRow dr in dt.Rows)
                data.Add(GetBlock(dr));

            return data;
        }
        public static BlockInfo GetBlock(long blockID)
        {
            string strSQL = "SELECT * " +
            " FROM Blocks " +
            " WHERE BlockID = " + blockID;

            DataTable dt = RunExecuteReader(strSQL);
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
            block.ClusterID = Convert.ToInt32(dr["ClusterID"]);
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
            DataTable dt = RunExecuteReader(strSQL);

            foreach(BlockInfo block in blocks)
            {
                BlockEnhancedInfo be = new BlockEnhancedInfo();
                be.Details = block.Details;
                be.DueDate = block.DueDate;
                be.EndDate = block.EndDate;
                be.HasDue = block.HasDue;
                be.ID = block.ID;
                be.ProjectID = block.ProjectID;
                be.ClusterID = block.ClusterID;
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
            DataTable dt = RunExecuteReader(strSQL);

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

            DataTable dt = RunExecuteReader(strSQL);

            foreach (DataRow dr in dt.Rows)
                segments.Add(GetSegment(dr));

            return segments;
        }
        public static List<BlockInfo> GetOldestBlocks(long projectID)
        {
            List<BlockInfo> blocks = new List<BlockInfo>();

            string strSQL = "SELECT * " +
                " FROM Blocks " +
                " WHERE Blocks.ProjectID = " + projectID +
                " AND StatusID = " + (int)DTC.StatusEnum.Running +
                " ORDER BY StartDate";

            DataTable dt = RunExecuteReader(strSQL);

            foreach (DataRow dr in dt.Rows)
                blocks.Add(GetBlock(dr));

            return blocks;
        }
        public static List<SegmentInfo> GetSegments(long blockID)
        {
            List<SegmentInfo> segmentsRaw = new List<SegmentInfo>();
            List<SegmentInfo> segmentsOrdered = new List<SegmentInfo>();

            string strSQL = "SELECT * " +
                " FROM Segments " +
                " WHERE BlockID = " + blockID;

            DataTable dt = RunExecuteReader(strSQL);

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

            DataTable dt = RunExecuteReader(strSQL);
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
            segment.Size = (DTC.SizeEnum)Convert.ToInt16(dr["Size"]);
            segment.StartDate = Convert.ToDateTime(dr["StartDate"]);
            segment.EndDate = Convert.ToDateTime(dr["EndDate"]);

            return segment;
        }
        public static SegmentEnhancedInfo GetSegmentEnhanced(DataRow dr)
        {
            SegmentInfo segmentBase = GetSegment(dr);
            SegmentEnhancedInfo sE = new SegmentEnhancedInfo();

            sE.ID = segmentBase.ID;
            sE.Title = segmentBase.Title;
            sE.Details = segmentBase.Details;
            sE.BlockID = segmentBase.BlockID;
            sE.Status = segmentBase.Status;
            sE.StartDate = segmentBase.StartDate;
            sE.ProjectName = Convert.ToString(dr["ProjectName"]);
            sE.BlockTitle = Convert.ToString(dr["BlockTitle"]);

            return sE;
        }
        public static string UpdateSegment(SegmentInfo segment)
        {
            List<GoalInfo> goalsOfSegment = Goals.GetGoals("SELECT * FROM Goals WHERE ItemID = " + segment.ID, false);
            foreach(GoalInfo goal in goalsOfSegment)
            {
                goal.Definition = segment.Title;
                Goals.UpdateGoal(goal);
            }

            List<TaskInfo> tasksOfSegment = Tasks.GetTasksOfSegment(segment.ID, TaskStatusEnum.All);
            foreach(TaskInfo task in tasksOfSegment)
            {
                task.Title = segment.Title;
                task.BlockID = segment.BlockID;
                Tasks.AddUpdateTask(task);  
            }

            string strSQL = "UPDATE Segments SET " +
                " Title = '" + DTC.Control.InputText(segment.Title, 255) + "'," +
                " BlockID = " + segment.BlockID + "," +
                " Details = '" + DTC.Control.InputText(segment.Details, 255) + "'," +
                " StartDate = " + DTC.ObtainGoodDT(segment.StartDate, true) + "," +
                " EndDate = " + DTC.ObtainGoodDT(segment.EndDate, true) + "," +
                " StatusID = " + (int)segment.Status + ", " +
                " Size = " + (int)segment.Size +
                " WHERE SegmentID = " + segment.ID;
            RunNonQuery(strSQL);

            return "OK";
        }
        public static string AddSegment(SegmentInfo segment)
        {
            int xIconID = Blocks.GetBlockXIcon(segment.BlockID);
            string strSQL = "INSERT Segments (BlockID, Title, Details, StartDate, EndDate, DueDate, HasDue, Size, StatusID, XIconID) VALUES (" +
                segment.BlockID + "," +
                "'" + DTC.Control.InputText(segment.Title, 255) + "'," +
                "'" + DTC.Control.InputText(segment.Details, 255) + "'," +      
                DTC.ObtainGoodDT(segment.StartDate, true) + "," +
                DTC.ObtainGoodDT(DateTime.Today, true) + "," +
                DTC.ObtainGoodDT(DateTime.Today, true) + "," +
                0 + "," +
                (int)segment.Size + "," +
                (int)DTC.StatusEnum.Running + "," +
                xIconID +
                ")";
            RunNonQuery(strSQL);

            return "OK";
        }
        public static void DeleteSegment(long segmentID)
        {
            string strSQL = "DELETE Segments WHERE SegmentID = " + segmentID;
            RunNonQuery(strSQL);
        }
        public static List<SegmentEnhancedInfo> GetSmallSegments()
        {
            List<SegmentEnhancedInfo> data = new List<SegmentEnhancedInfo>();

            string strSQL = "SELECT Segments.*, Blocks.Title AS BlockTitle, Projects.ProjectName " +
                " FROM Segments " +
                " INNER JOIN Blocks ON Segments.BlockID = Blocks.BlockID " +
                " INNER JOIN Projects ON Blocks.ProjectID = Projects.ProjectID " +
                " WHERE Segments.StatusID = " + (int)DTC.StatusEnum.Running +
                " AND Segments.Size = " + (int)DTC.SizeEnum.Small +
                " AND Projects.IsActionable = 1" +
                " AND Projects.StatusID = 1" +
                " ORDER BY Segments.StartDate DESC";

            DataTable dt = RunExecuteReader(strSQL);
            foreach (DataRow dr in dt.Rows)
                data.Add(GetSegmentEnhanced(dr));

            return data;
        }

        public static Dictionary<string, int> GetRealMinutesPerWeekOLD(int projectID, int nTop)
        {
            Dictionary<string, int> data = new Dictionary<string, int>();

            string strSQL = "SELECT TOP (" + nTop + ") SUM(RealTime) AS TotalPerWeek, " +
                " CONVERT(varchar, { fn YEAR(TaskDate) }) + CONVERT(varchar, { fn WEEK(TaskDate) }) AS TheWeek " +
                " FROM Tasks " +
                " WHERE ProjectID = " + projectID +
                " GROUP BY CONVERT(varchar, { fn YEAR(TaskDate) }) +CONVERT(varchar, { fn WEEK(TaskDate) }) " +
                " ORDER BY TheWeek DESC";
            DataTable dt = RunExecuteReader(strSQL);

            foreach (DataRow dr in dt.Rows)
            {
                data.Add(Convert.ToString(dr["TheWeek"]), Convert.ToInt16(dr["TotalPerWeek"]));
            }

            return data;
        }

        public static Dictionary<string, int> GetRealMinutesPerWeek(int projectID, int nTop)
        {
            Dictionary<string, int> data = new Dictionary<string, int>();
            List<WeekInfo> weeks = GetWeeks(projectID, nTop);

            string strSQL = "SELECT * FROM Tasks " +
                " WHERE ProjectID = " + projectID +
                " ORDER BY TaskDate DESC";
            DataTable dt = RunExecuteReader(strSQL);

            foreach(WeekInfo week in weeks)
            {
                if (!data.ContainsKey(week.GetYearWeekKey()))
                    data.Add(week.GetYearWeekKey(), 0);
            }

            foreach (DataRow dr in dt.Rows)
            {
                DateTime theDate = Convert.ToDateTime(dr["TaskDate"]);
                WeekInfo week = weeks.Find(i=>i.StartDate <= theDate && i.EndDate >= theDate);
                if(week != null)
                {
                    string key = week.GetYearWeekKey();
                    if (data.ContainsKey(key))
                    {
                        data[key] += Convert.ToInt16(dr["RealTime"]);
                    }
                }
            }

            return data;
        }
        public static List<WeekInfo> GetWeeks(int projectID, int nTop)
        {
            List<WeekInfo> data = new List<WeekInfo>();
            ProjectInfo project = DB.Projects.GetProject(projectID);

            bool isOK = false;
            WeekInfo week = new WeekInfo(DateTime.Today);
            int count = 0;
            while (!isOK)
            {
                data.Add(week);

                week = DTC.GetPreviousWeek(week);
                count++;
                if (count >= nTop || week.StartDate < project.StartDate)
                    isOK = true;
            }

            return data;
        }

        public enum TaskStatusEnum
        {
            Running, Completed, All
        }
        public class Tasks
        {
            public static long AddUpdateTask(TaskInfo task)
            {
                string SQL = "";

                if (task.ID == 0)
                {
                    SQL = "INSERT INTO Tasks " +
                        " (Title, Details, IsCompleted, IsPrivilaged, " +
                        " ProjectGroupID, ProjectID, SubProjectID, ClusterID, BlockID, SegmentID," +
                        " StartDate, EndDate, TaskDate, PlannedTime, RealTime," +
                        " IsFloating, IsThing, CanBeep, OrderGeneral, OrderActive, " +
                        " Seperator, SeperatorHour, TemplateID" +
                        " ) VALUES (" +
                        "'" + DTC.Control.InputText(task.Title, 255) + "'," +
                        "'" + DTC.Control.InputText(task.Details, 999) + "'," +
                        Convert.ToInt16(task.IsCompleted) + "," +
                        Convert.ToInt16(task.IsPrivilaged) + "," +
                        task.ProjectGroupID + "," +
                        task.ProjectID + "," +
                        task.SubProjectID + "," +
                        task.ClusterID + "," +
                        task.BlockID + "," +
                        task.SegmentID + "," +
                        DTC.Date.ObtainGoodDT(task.StartDate, true) + "," +
                        DTC.Date.ObtainGoodDT(task.EndDate, true) + "," +
                        DTC.Date.ObtainGoodDT(task.TaskDate, true) + "," +
                        task.PlannedTime + "," +
                        task.RealTime + "," +
                        Convert.ToInt16(task.IsFloating) + "," +
                        Convert.ToInt16(task.IsThing) + "," +
                        Convert.ToInt16(task.CanBeep) + "," +
                        task.OrderGeneral + "," +
                        task.OrderActive + "," +
                        (int)task.Seperator + "," +
                        "'" + task.SeperatorHour + "', " +
                        task.TemplateID +
                        ") SELECT SCOPE_IDENTITY() AS TaskID";
                    task.ID = RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Tasks SET " +
                        " Title = '" + DTC.Control.InputText(task.Title, 255) + "'," +
                        " Details = '" + DTC.Control.InputText(task.Details, 999) + "'," +
                        " IsCompleted = " + Convert.ToInt16(task.IsCompleted) + "," +
                        " IsPrivilaged = " + Convert.ToInt16(task.IsPrivilaged) + "," +
                        " ProjectGroupID = " + task.ProjectGroupID + "," +
                        " ProjectID = " + task.ProjectID + "," +
                        " SubProjectID = " + task.SubProjectID + "," +
                        " ClusterID = " + task.ClusterID + "," +
                        " BlockID = " + task.BlockID + "," +
                        " SegmentID = " + task.SegmentID + "," +
                        " StartDate = " + DTC.Date.ObtainGoodDT(task.StartDate, true) + "," +
                        " EndDate = " + DTC.Date.ObtainGoodDT(task.EndDate, true) + "," +
                        " TaskDate = " + DTC.Date.ObtainGoodDT(task.TaskDate, true) + "," +
                        " PlannedTime = " + task.PlannedTime + "," +
                        " RealTime = " + task.RealTime + "," +
                        " IsFloating = " + Convert.ToInt16(task.IsFloating) + "," +
                        " IsThing = " + Convert.ToInt16(task.IsThing) + "," +
                        " CanBeep = " + Convert.ToInt16(task.CanBeep) + "," +
                        " OrderGeneral = " + task.OrderGeneral + "," +
                        " OrderActive = " + task.OrderActive + "," +
                        " Seperator = " + (int)task.Seperator + "," +
                        " SeperatorHour = '" + task.SeperatorHour + "'," +
                        " TemplateID = " + task.TemplateID +
                        " WHERE TaskID = " + task.ID;
                    RunNonQuery(SQL);
                }
                 
                SQL = "UPDATE Projects " +
                    " SET LastTaskDate = " + DTC.ObtainGoodDT(DateTime.Today, true) +
                    " WHERE ProjectID = " + task.ProjectID;
                RunNonQuery(SQL);


                return task.ID;
            }
            public static long UpdateTaskOrder(long taskID, int order)
            {
                string  SQL = "UPDATE Tasks SET " +
                        " OrderGeneral = " + order + "," +
                        " OrderActive = " + order +  
                        " WHERE TaskID = " + taskID;
                try
                {
                    RunNonQuery(SQL);
                    return taskID;

                }
                catch
                {
                    return 0;
                }
                
            }
            public static bool DeleteTask(long taskID)
            {
                string strSQL = "DELETE CoTasks WHERE TaskID = " + taskID;
                RunNonQuery(strSQL);

                strSQL = "DELETE Tasks WHERE TaskID = " + taskID;
                RunNonQuery(strSQL);

                return true;
            }
            public static bool DeleteTasks(string strTasks)
            {
                List<string> taskIDs =  DTC.SplitInto(strTasks, '|');

                foreach(string taskID in taskIDs)
                {
                    string strSQL = "DELETE CoTasks WHERE TaskID = " + taskID;
                    RunNonQuery(strSQL);

                    strSQL = "DELETE Tasks WHERE TaskID = " + taskID;
                    RunNonQuery(strSQL);
                }

                return true;
            }
            public static void CloneTask(List<TaskInfo> tasks, TaskInfo task)
            {
                TaskInfo newTask = task;

                newTask.ID = 0;
                newTask.RealTime = 0;
                newTask.IsCompleted = false;

                AddUpdateTask(newTask);
                InsertTaskPriority(tasks, newTask, task.OrderActive + 1);
            }
            public static List<TaskInfo> GetTasksFromStringSplit(string strTasks)
            {
                List<TaskInfo> data = new List<TaskInfo>();
                List<string> taskIDs = DTC.SplitInto(strTasks, '|');

                string strSQL = "SELECT * FROM Tasks WHERE TaskID = -1 ";
                foreach (string taskID in taskIDs)
                {
                    strSQL += " OR TaskID = " + taskID;
                }
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetTask(dr));

                return data;
            }
            public static bool CloneTasks(string strTasks)
            {
                List<TaskInfo> tasks = GetTasksFromStringSplit(strTasks);

                foreach (TaskInfo task in tasks)
                {
                    TaskInfo newTask = task;

                    newTask.ID = 0;
                    newTask.RealTime = 0;
                    newTask.IsCompleted = false;

                    AddUpdateTask(newTask);
                }

                return true;
            }
            public static bool SegmentTask(TaskInfo task, DateTime destDate)
            {
                if (task.RealTime < task.PlannedTime)
                {
                    TaskInfo newTask = (TaskInfo)((ICloneable)task).Clone();

                    newTask.ID = 0;
                    newTask.PlannedTime = task.PlannedTime - task.RealTime;
                    newTask.RealTime = 0;
                    newTask.IsCompleted = false;
                    newTask.TaskDate = destDate;

                    // create the new task
                    AddUpdateTask(newTask);
                    List<TaskInfo> tasks = GetTasks(destDate, destDate, TaskStatusEnum.Running);
                    InsertTaskPriority(tasks, newTask, 1);

                    // complete the original task
                    task.IsCompleted = true;
                    AddUpdateTask(task);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static TaskInfo GetTask(DataRow dr)
            {
                TaskInfo info = new TaskInfo();

                if(dr != null)
                {
                    info.ID = Convert.ToInt32(dr["TaskID"]);
                    info.Title = Convert.ToString(dr["Title"]);
                    info.Details = Convert.ToString(dr["Details"]);
                    info.IsCompleted = Convert.ToBoolean(dr["IsCompleted"]);
                    info.IsPrivilaged = Convert.ToBoolean(dr["IsPrivilaged"]);
                    info.ProjectGroupID = Convert.ToInt32(dr["ProjectGroupID"]);
                    info.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                    info.SubProjectID = Convert.ToInt32(dr["SubProjectID"]);
                    info.ClusterID = Convert.ToInt32(dr["ClusterID"]);
                    info.BlockID = Convert.ToInt32(dr["BlockID"]);
                    info.SegmentID = Convert.ToInt32(dr["SegmentID"]);
                    info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                    info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                    info.TaskDate = Convert.ToDateTime(dr["TaskDate"]);
                    info.PlannedTime = Convert.ToInt16(dr["PlannedTime"]);
                    info.RealTime = Convert.ToInt16(dr["RealTime"]);
                    info.IsFloating = Convert.ToBoolean(dr["IsFloating"]);
                    info.IsThing = Convert.ToBoolean(dr["IsThing"]);
                    info.CanBeep = Convert.ToBoolean(dr["CanBeep"]);
                    info.OrderGeneral = Convert.ToInt16(dr["OrderGeneral"]);
                    info.OrderActive = Convert.ToInt16(dr["OrderActive"]);
                    info.Seperator = (TaskInfo.SeperatorEnum)Convert.ToInt16(dr["Seperator"]);
                    info.SeperatorHour = Convert.ToString(dr["SeperatorHour"]);
                    info.TemplateID = Convert.ToInt32(dr["TemplateID"]);
                }

                return info;
            }
            public static TaskInfo GetTaskPartial(DataRow dr)
            {
                TaskInfo info = new TaskInfo();
                info.Title = Convert.ToString(dr["Title"]);
                info.IsThing = Convert.ToBoolean(dr["IsThing"]);
                info.IsFloating = Convert.ToBoolean(dr["IsFloating"]);
                info.CanBeep = Convert.ToBoolean(dr["CanBeep"]);
                info.PlannedTime = Convert.ToInt16(dr["PlannedTime"]);
                info.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                info.ProjectGroupID = Convert.ToInt32(dr["ProjectGroupID"]);

                return info;
            }
            public static TaskInfo GetTask(long taskID)
            {
                TaskInfo task = new TaskInfo();
                string strSQL = "SELECT * FROM Tasks" +
                    " WHERE TaskID = " + taskID;

                DataTable dt = RunExecuteReader(strSQL);
                
                return GetTask(GetSingleDR(dt, true));
            }
            public static List<TaskInfo> GetTasks(string strSQL)
            {
                List<TaskInfo> data = new List<TaskInfo>();

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetTask(dr));
                
                return data;
            }
            public static List<TaskInfo> GetTasks(long projectID, TaskStatusEnum taskStatus, int numTasks)
            {
                string filterSQL = "";

                if (taskStatus == TaskStatusEnum.Running)
                    filterSQL = " AND IsCompleted=0";
                else if (taskStatus == TaskStatusEnum.Completed)
                    filterSQL = " AND IsCompleted=1";
                else if (taskStatus == TaskStatusEnum.All)
                    filterSQL = "";

                string SQL = "SELECT TOP " + numTasks + " * FROM Tasks " +
                    " WHERE ProjectID = " + projectID +
                    filterSQL +
                    " ORDER BY TaskDate DESC";

                return GetTasks(SQL);
            }
            public static List<TaskInfo> GetOldestActionableTasks()
            {
                string strSQL = "SELECT Tasks.*" +
                    " FROM Tasks INNER JOIN Projects ON Tasks.ProjectID = Projects.ProjectID" +
                    " WHERE Projects.IsActionable = 1 " +
                    " AND  Tasks.IsCompleted = 0 " +
                    " AND Tasks.TemplateID = 0 " +
                    " AND Projects.StatusID = " + (int)DTC.StatusEnum.Running +
                    " ORDER BY Tasks.StartDate";

                return GetTasks(strSQL);
            }
            public static List<TaskInfo> GetTasksOfBlock(long blockID, TaskStatusEnum taskStatus)
            {
                string filterSQL = "";

                if (taskStatus == TaskStatusEnum.Running)
                    filterSQL = " AND IsCompleted=0";
                else if (taskStatus == TaskStatusEnum.Completed)
                    filterSQL = " AND IsCompleted=1";
                else if (taskStatus == TaskStatusEnum.All)
                    filterSQL = "";

                string strSQL = "SELECT * FROM Tasks " +
                    " WHERE BlockID = " + blockID +
                    filterSQL +
                    " ORDER BY IsCompleted ASC, OrderGeneral ASC";

                return GetTasks(strSQL);
            }
            public static List<TaskInfo> GetTasksOfSegment(long segmentID, TaskStatusEnum taskStatus)
            {
                string filterSQL = "";
                
                if (taskStatus == TaskStatusEnum.Running)
                    filterSQL = " AND IsCompleted=0";
                else if (taskStatus == TaskStatusEnum.Completed)
                    filterSQL = " AND IsCompleted=1";
                else if (taskStatus == TaskStatusEnum.All)
                    filterSQL = "";

                string strSQL = "SELECT * FROM Tasks " +
                    " WHERE SegmentID = " + segmentID +
                    filterSQL +
                    " ORDER BY IsCompleted ASC, OrderGeneral ASC";

                return GetTasks(strSQL);
            }
            public static List<TaskInfo> GetTasks(DateTime startDate, DateTime endDate, TaskStatusEnum taskStatus)
            {
                string filterSQL = "";

                if (taskStatus == TaskStatusEnum.Running)
                    filterSQL = " AND IsCompleted=0";
                else if (taskStatus == TaskStatusEnum.Completed)
                    filterSQL = " AND IsCompleted=1";
                else if (taskStatus == TaskStatusEnum.All)
                    filterSQL = "";

                string strSQL = "SELECT * FROM Tasks " +
                    " WHERE TaskDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TaskDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " AND TemplateID = 0" +
                    filterSQL +
                    " ORDER BY IsCompleted ASC, OrderActive ASC";

                return GetTasks(strSQL);
            }
            public static List<TaskInfo> GetTasks(DateTime theDate, TaskStatusEnum taskStatus)
            {
                string filterSQL = "";

                if (taskStatus == TaskStatusEnum.Running)
                    filterSQL = " AND IsCompleted=0";
                else if (taskStatus == TaskStatusEnum.Completed)
                    filterSQL = " AND IsCompleted=1";
                else if (taskStatus == TaskStatusEnum.All)
                    filterSQL = "";

                string strSQL = "SELECT * FROM Tasks " +
                    " WHERE TaskDate = " + DTC.Date.ObtainGoodDT(theDate, true) +
                    filterSQL +
                    " ORDER BY IsCompleted ASC, OrderActive ASC";

                return GetTasks(strSQL);
            }
            public static List<TaskInfo> GetPrivilagedTasks()
            {
                string strSQL = "SELECT * FROM Tasks " +
                    " WHERE IsCompleted = 0" +
                    " AND IsPrivilaged = 1" +
                    " AND ProjectID > 0 " +
                    " ORDER BY ProjectID";

                return GetTasks(strSQL);
            }
            public static List<TaskInfo> GetActionableTasks()
            {
                string strSQL = "SELECT Tasks.* FROM Tasks, Projects " +
                    " WHERE Tasks.ProjectID = Projects.ProjectID " +
                    " AND Projects.IsActionable = 1 " +
                    " AND Tasks.IsCompleted = 0 " +
                    " AND Tasks.ProjectID > 0 " +
                    " AND Projects.StatusID = " + (int)DTC.StatusEnum.Running +
                    " ORDER BY Tasks.ProjectID";

                return GetTasks(strSQL);
            }

            /// <summary>
            /// Gets all the most popular task titles
            /// </summary>
            /// <param name="numMaxTasks">Maximum number of titles</param>
            /// <param name="nDays">Go back nDays</param>
            /// <returns>List of task titles</returns>
            public static List<TaskInfo> GetTaskHistory(int numMaxTasks, int nDays)
            {
                List<TaskInfo> taskHistory = new List<TaskInfo>();

                string strSQL = "SELECT TOP (" + numMaxTasks + ") COUNT(Title) AS TOPLAM," +
                    " Title, ProjectGroupID, ProjectID, PlannedTime, Leverage, IsFloating, IsThing, CanBeep" +
                    " FROM Tasks" +
                    " WHERE TaskDate >= " + DTC.Date.ObtainGoodDT(DateTime.Today.AddDays(-nDays), true) +
                    " GROUP BY Title, ProjectGroupID, ProjectID, PlannedTime, Leverage, " +
                    " IsFloating, IsThing, CanBeep" +
                    " ORDER BY TOPLAM DESC, Title ASC";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    taskHistory.Add(GetTaskPartial(dr));

                // now sort them alphabetically
                IEnumerable<TaskInfo> sortedTasks =
                  from task in taskHistory
                  orderby task.Title ascending
                  select task;

                return new List<TaskInfo>(sortedTasks);
            }
            public static Dictionary<MonthInfo, float> GetTotalsOfProject(int projectID)
            {
                Dictionary<MonthInfo, float> totals = new Dictionary<MonthInfo, float>();

                string strSQL = "SELECT CONVERT(float, SUM(RealTime)) / 60 AS TOTAL," +
                    " MONTH(TaskDate) AS THEMONTH, YEAR(TaskDate) AS THEYEAR" +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + projectID +
                    " GROUP BY YEAR(TaskDate), MONTH(TaskDate)" +
                    " ORDER BY THEYEAR DESC, THEMONTH DESC";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    MonthInfo month = new MonthInfo(new DateTime(Convert.ToInt32(dr["THEYEAR"]), Convert.ToInt32(dr["THEMONTH"]), 1));
                    totals.Add(month, Convert.ToSingle(dr["TOTAL"]));
                }
                
                return totals;
            }
            /// <summary>
            /// Populates the totals dictionary of days between the date interval.
            /// !WARNING : The day numbers should be unique, i.e., there should be one 23rd. 
            /// This method is applicable to below usage : 
            /// - Days of a month
            /// - A few days in a month
            /// - A single day (if startDate = endDate)
            /// </summary>
            /// <param name="days">Days dictionary</param>
            /// <param name="startDate">Start date of the interval</param>
            /// <param name="endDate">End date of the interval</param>
            /// <returns></returns>
            public static List<DayInfo> UpdateDailyTotals(List<DayInfo> days, DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT CONVERT(float, SUM(Tasks.RealTime)) / 60 AS TOTAL," +
                    " ProjectTypes.ProjectTypeOrder, Tasks.TaskDate" +
                    " FROM Tasks" +
                    " INNER JOIN ProjectGroups ON Tasks.ProjectGroupID = ProjectGroups.ProjectGroupID" +
                    " INNER JOIN ProjectTypes ON ProjectGroups.ProjectTypeID = ProjectTypes.ProjectTypeID" +
                    " WHERE Tasks.TaskDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND Tasks.TaskDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " GROUP BY Tasks.TaskDate, ProjectTypes.ProjectTypeOrder" +
                    " ORDER BY Tasks.TaskDate, ProjectTypes.ProjectTypeOrder";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    for (int i = 0; i < days.Count; i++)
                    {
                        if (days[i].TheDate.Date == Convert.ToDateTime(dr["TaskDate"]).Date)
                            days[i].Totals.Add(Convert.ToInt32(dr["ProjectTypeOrder"]), Convert.ToSingle(dr["TOTAL"]));
                    }
                }
                
                return days;
            }
            public static List<MonthInfo> UpdateMonthlyTotals(List<MonthInfo> months, DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT CONVERT(float, SUM(Tasks.RealTime)) / 60 AS TOTAL," +
                    " ProjectTypes.ProjectTypeID, MONTH(Tasks.TaskDate) AS THE_MONTH " +
                    " FROM Tasks" +
                    " INNER JOIN ProjectGroups ON Tasks.ProjectGroupID = ProjectGroups.ProjectGroupID" +
                    " INNER JOIN ProjectTypes ON ProjectGroups.ProjectTypeID = ProjectTypes.ProjectTypeID" +
                    " WHERE Tasks.TaskDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND Tasks.TaskDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " GROUP BY MONTH(Tasks.TaskDate), ProjectTypes.ProjectTypeID " +
                    " ORDER BY THE_MONTH, ProjectTypes.ProjectTypeID";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    int monthNO = Convert.ToInt16(dr["THE_MONTH"]);
                    int projectTypeID = Convert.ToInt16(dr["ProjectTypeID"]);
                    float value = Convert.ToSingle(dr["TOTAL"]);

                    foreach (MonthInfo m in months)
                    {
                        if (m.Month == monthNO)
                        {
                            if (!m.Totals.ContainsKey(projectTypeID))
                                m.Totals.Add(projectTypeID, 0);
                            m.Totals[projectTypeID] = value;
                        }
                    }
                }
                
                return months;
            }
            public static List<DayInfo> UpdateThings(List<DayInfo> days, DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT COUNT(TaskID) AS TOTAL, TaskDate" +
                    " FROM Tasks" +
                    " WHERE TaskDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TaskDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " AND IsCompleted = 1 AND IsThing = 1" +
                    " GROUP BY TaskDate ORDER BY TaskDate";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    for (int i = 0; i < days.Count; i++)
                    {
                        if (days[i].TheDate.Date == Convert.ToDateTime(dr["TaskDate"]).Date)
                            days[i].NumThings = Convert.ToSingle(dr["TOTAL"]);
                    }
                }
                
                return days;
            }
            public static List<DayInfo> UpdateIdeas(List<DayInfo> days, DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT COUNT(IdeaID) AS TOTAL, CreationDate" +
                    " FROM Ideas" +
                    " WHERE CreationDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND CreationDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " GROUP BY CreationDate" +
                    " ORDER BY CreationDate";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    for (int i = 0; i < days.Count; i++)
                    {
                        if (days[i].TheDate.Date == Convert.ToDateTime(dr["CreationDate"]).Date)
                        {
                            days[i].NumIdeas = Convert.ToSingle(dr["TOTAL"]);
                        }
                    }
                }
                
                return days;
            }
            public static List<DayInfo> UpdateTasksCompletedPerDay(List<DayInfo> days, DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT COUNT(TaskID) AS TOTAL, TaskDate" +
                    " FROM Tasks" +
                    " WHERE TaskDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TaskDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " AND IsThing = 1 " +
                    " AND IsCompleted = 1" +
                    " GROUP BY TaskDate" +
                    " ORDER BY TaskDate";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    for (int i = 0; i < days.Count; i++)
                    {
                        if (days[i].TheDate.Date == Convert.ToDateTime(dr["TaskDate"]).Date)
                        {
                            days[i].NumTodos = Convert.ToSingle(dr["TOTAL"]);
                        }
                    }
                }

                return days;
            }

            public static void InsertTaskPriority(List<TaskInfo> tasks, TaskInfo task, int order)
            {
                foreach (TaskInfo info in tasks)
                {
                    if (info.OrderActive >= order) info.OrderActive++;
                    ChangeTaskOrderActive(info, info.OrderActive);
                }
                ChangeTaskOrderActive(task, order);
            }
            public static void ChangeTaskOrderActive(TaskInfo task, int order)
            {
                string strSQL = "UPDATE Tasks SET " +
                    " OrderActive = " + order +
                    " WHERE TaskID = " + task.ID;
                RunNonQuery(strSQL);
            }

            public static void DeleteLock()
            {
                RunNonQuery("DELETE FROM TaskLock");
            }
            public static bool LockTask(long taskID, DateTime lockInstance)
            {
                DeleteLock();
                if (taskID > 0)
                {
                    string strSQL = "INSERT INTO TaskLock" +
                        " (TaskID,LockInstance)" +
                        " VALUES (" +
                        taskID + "," +
                        DTC.Date.ObtainGoodDT(lockInstance, false) +
                        ")";
                    RunNonQuery(strSQL);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static bool GetLockInstance(out TaskInfo task, out DateTime lockInstance)
            {
                task = new TaskInfo();
                lockInstance = DateTime.Now;

                string strSQL = "SELECT * FROM TaskLock";
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count == 1)
                {
                    DataRow dr = dt.Rows[0];
                    task = Tasks.GetTask(Convert.ToInt32(dr["TaskID"]));
                    lockInstance = Convert.ToDateTime(dr["LockInstance"]);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static Dictionary<int, Dictionary<int, int>> GetNumBigTasksOfProjects()
            {
                Dictionary<int, Dictionary<int, int>> dict = new Dictionary<int, Dictionary<int, int>>();

                string strSQL = "SELECT ProjectID, IsCompleted, COUNT(TaskID) AS NUM_TASKS" +
                    " FROM Tasks" +
                    " WHERE (IsThing = 1) AND (ProjectID > 0)" +
                    " GROUP BY ProjectID, IsCompleted" +
                    " ORDER BY ProjectID, IsCompleted";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    int projectID = Convert.ToInt32(dr["ProjectID"]);
                    int status = Convert.ToInt16(dr["IsCompleted"]);
                    int numTasks = Convert.ToInt32(dr["NUM_TASKS"]);

                    if (!dict.ContainsKey(projectID)) dict.Add(projectID, new Dictionary<int, int>());
                    if (!dict[projectID].ContainsKey(status)) dict[projectID].Add(status, numTasks);
                }
                
                return dict;
            }

            public static void SetTaskRunning(long taskID)
            {
                RunNonQuery("DELETE FROM TaskRunning");
                RunNonQuery("INSERT INTO TaskRunning (TaskID) VALUES (" + taskID + ")");
            }
            public static long GetTaskRunning()
            {
                long taskID = 0;
                string strSQL = "SELECT * FROM TaskRunning";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["TaskID"] != DBNull.Value)
                    {
                        if (DTC.IsNumeric(dr["TaskID"]))
                            taskID = Convert.ToInt32(dr["TaskID"]);
                    }
                }
                

                return taskID;
            }
            public static GoalInfo GetGoalRelatedTo(TaskInfo task)
            {
                GoalInfo goal = new GoalInfo();

                string strSQL = "SELECT * FROM Goals WHERE ItemID = " + task.ID;
                DataTable dt = RunExecuteReader(strSQL);
                
                goal = Goals.GetGoal(GetSingleDR(dt, false), true);
                
                return goal;
            }
            public static TaskInfo GetTaskRelatedToGoal(GoalInfo goal)
            {
                return DB.Tasks.GetTask(goal.ItemID);
            }

            public static float GetTotalHoursIncludingCoTasks(int projectID, bool isIncludeCoTasks)
            {
                float result = 0;

                string strSQL = "SELECT SUM(RealTime) AS TOTAL " +
                    " FROM Tasks " +
                    " WHERE ProjectID = " + projectID;
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count == 1 && dt.Rows[0][0] != DBNull.Value && DTC.IsNumeric(dt.Rows[0][0]))
                    result += Convert.ToSingle(dt.Rows[0][0]) / 60.0f;

                if(isIncludeCoTasks)
                {
                    strSQL = "SELECT SUM(Tasks.RealTime) AS TOTAL " +
                    " FROM CoTasks " +
                    " INNER JOIN Tasks ON CoTasks.TaskID = Tasks.TaskID " +
                    " WHERE CoTasks.ProjectID = " + projectID;
                    dt = RunExecuteReader(strSQL);
                    if (dt.Rows.Count == 1 && dt.Rows[0][0] != DBNull.Value && DTC.IsNumeric(dt.Rows[0][0]))
                        result += Convert.ToSingle(dt.Rows[0][0]) / 60.0f;
                }

                return result;
            }

            public static bool PostponeTasks(string strTasks, DateTime newDate)
            {
                List<string> taskIDs = DTC.SplitInto(strTasks, '|');
                DayInfo day = Days.GetDay(newDate, true);

                foreach (string strTaskID in taskIDs)
                {
                    long taskID = Convert.ToInt32(strTaskID);
                    TaskInfo task = GetTask(taskID);
                    long segmentID = task.SegmentID;

                    if(segmentID > 0)
                    {
                        string strSQLGoal = "UPDATE Goals SET" +
                        " DateStart = " + DTC.Date.ObtainGoodDT(newDate, true) + "," +
                        " DateEnd = " + DTC.Date.ObtainGoodDT(newDate, true) + "," +
                        " DateDue = " + DTC.Date.ObtainGoodDT(newDate, true) + ", " +
                        " OwnerID = " + day.DayID +
                        " WHERE ItemID = " + segmentID;
                        RunNonQuery(strSQLGoal);
                    }

                    string strSQLTask = "UPDATE Tasks SET " +
                        " StartDate = " + DTC.Date.ObtainGoodDT(newDate, true) + "," +
                        " EndDate = " + DTC.Date.ObtainGoodDT(newDate, true) + "," +
                        " TaskDate = " + DTC.Date.ObtainGoodDT(newDate, true) + 
                        " WHERE TaskID = " + strTaskID;
                    RunNonQuery(strSQLTask);
                }

                return true;
            }
            public static bool CompleteTasks(string strTasks)
            {
                List<string> taskIDs = DTC.SplitInto(strTasks, '|');

                foreach (string taskID in taskIDs)
                {
                    TaskInfo task = GetTask(Convert.ToInt32(taskID));
                    float realTime = task.RealTime;
                    if(realTime == 0)
                    {
                        realTime = task.PlannedTime;
                    }

                    string strSQL = "UPDATE Tasks SET " +
                         " IsCompleted = 1," +
                         " RealTime = " + realTime + 
                         " WHERE TaskID = " + taskID;
                    RunNonQuery(strSQL);
                }

                return true;
            }
            public static List<TaskInfo> CreateTasksWithTemplate(int templateID, DateTime thedate)
            {
                List<TaskInfo> tasksOfTemplate = TaskTemplates.GetTasksWithTemplate(templateID);
                List<TaskInfo> tasksNew = new List<TaskInfo>();
                foreach(TaskInfo task in tasksOfTemplate)
                {
                    task.ID = 0;
                    task.TemplateID = 0;
                    task.StartDate = thedate;
                    task.EndDate= thedate;
                    task.TaskDate = thedate;
                    task.IsCompleted = false;
                    task.RealTime = 0;

                    tasksNew.Add(task);
                    AddUpdateTask(task);
                }

                return tasksNew;
            }
        }

        public class CoTasks
        {
            public static long AddUpdateCoTask(CoTaskInfo coTask)
            {
                string SQL = "";

                if (coTask.CoTaskID == 0)
                {
                    SQL = "INSERT INTO CoTasks " +
                        " (TaskID, CoTaskTitle, ProjectGroupID, ProjectID" +
                        " ) VALUES (" +
                        coTask.TaskID + "," +
                        "'" + DTC.Control.InputText(coTask.Title, 99) + "'," +
                        coTask.ProjectGroupID + "," +
                        coTask.ProjectID + 
                        ") SELECT SCOPE_IDENTITY() AS CoTaskID";
                    coTask.CoTaskID = RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE CoTasks SET " +
                        " TaskID = " + coTask.TaskID + "," +
                        " CoTaskTitle = '" + DTC.Control.InputText(coTask.Title, 99) + "'," +
                        " ProjectGroupID = " + coTask.ProjectGroupID + "," +
                        " ProjectID = " + coTask.ProjectID + 
                        " WHERE CoTaskID = " + coTask.CoTaskID;
                    RunNonQuery(SQL);
                }

                return coTask.CoTaskID;
            }
            public static bool DeleteCoTask(CoTaskInfo coTask)
            {
                string strSQL = "DELETE CoTasks WHERE CoTaskID = " + coTask.CoTaskID;
                RunNonQuery(strSQL);

                return true;
            }
            public static CoTaskInfo GetCoTask(DataRow dr)
            {
                CoTaskInfo info = new CoTaskInfo();

                info.CoTaskID = Convert.ToInt32(dr["CoTaskID"]);
                info.TaskID = Convert.ToInt32(dr["TaskID"]);
                info.Title = Convert.ToString(dr["CoTaskTitle"]);                
                info.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                info.ProjectGroupID = Convert.ToInt32(dr["ProjectGroupID"]);

                return info;
            }
            public static Dictionary<long, CoTaskInfo> GetCoTasks(string strSQL)
            {
                Dictionary<long, CoTaskInfo> dict = new Dictionary<long, CoTaskInfo>();
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    dict.Add(Convert.ToInt32(dr["CoTaskID"]), GetCoTask(dr));

                return dict;
            }
            public static Dictionary<long, CoTaskInfo> GetCoTasks(TaskInfo task)
            {
                string SQL = "SELECT * FROM CoTasks" +
                    " WHERE TaskID = " + task.ID +
                    " ORDER BY CoTaskTitle";
                return GetCoTasks(SQL);
            }
        }

        public class TaskTemplates
        {
            public static int AddUpdateTaskTemplate(TaskTemplateInfo taskTemplate)
            {
                string SQL = "";

                if (taskTemplate.ID == 0)
                {
                    SQL = "INSERT INTO TaskTemplates " +
                        " (TaskTemplateName,Details" +
                        " ) VALUES (" +
                        "'" + DTC.Control.InputText(taskTemplate.Name, 20) + "'," +
                        "'" + DTC.Control.InputText(taskTemplate.Details, 99) + "'" +
                        ") SELECT SCOPE_IDENTITY() AS TaskTemplateID";
                    taskTemplate.ID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE TaskTemplates SET " +
                        " TaskTemplateName = '" + DTC.Control.InputText(taskTemplate.Name, 20) + "'," +
                        " Details = '" + DTC.Control.InputText(taskTemplate.Details, 99) + "'" +
                        " WHERE TaskTemplateID = " + taskTemplate.ID;
                    RunNonQuery(SQL);
                }

                return taskTemplate.ID;
            }
            public static bool DeleteTaskTemplate(TaskTemplateInfo taskTemplate)
            {
                string SQL = "DELETE TaskTemplateLog WHERE TaskTemplateID = " + taskTemplate.ID;
                RunNonQuery(SQL);

                SQL = "DELETE TaskTemplates WHERE TaskTemplateID = " + taskTemplate.ID;
                RunNonQuery(SQL);

                return true;
            }
            public static void DeleteTasksOfTemplate(TaskTemplateInfo taskTemplate)
            {
                if (taskTemplate.ID > 0)
                {
                    List<TaskInfo> tasks = Tasks.GetTasks("SELECT * FROM Tasks WHERE TemplateID = " + taskTemplate.ID);
                    foreach (TaskInfo task in tasks)
                        RunNonQuery("DELETE CoTasks WHERE TaskID=" + task.ID);

                    string SQL = "DELETE Tasks WHERE TemplateID = " + taskTemplate.ID;
                    RunNonQuery(SQL);
                }
            }
            public static List<TaskInfo> GetTasksWithTemplate(int templateID)
            {
                string SQL = "SELECT * FROM Tasks " +
                    " WHERE TemplateID = " + templateID +
                    " ORDER BY OrderActive";
                return Tasks.GetTasks(SQL);
            }
            public static TaskTemplateInfo GetTaskTemplate(DataRow dr)
            {
                TaskTemplateInfo info = new TaskTemplateInfo();

                info.ID = Convert.ToInt32(dr["TaskTemplateID"]);
                info.Name = Convert.ToString(dr["TaskTemplateName"]);
                if (dr["Details"] != DBNull.Value) info.Details = Convert.ToString(dr["Details"]);

                return info;
            }
            private static List<TaskTemplateInfo> GetTaskTemplates(string strSQL)
            {
                List<TaskTemplateInfo> data = new List<TaskTemplateInfo>();
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    data.Add(GetTaskTemplate(dr));
                }
                
                return data;
            }
            public static List<TaskTemplateInfo> GetTaskTemplates()
            {
                string SQL = "SELECT * FROM TaskTemplates ORDER BY TaskTemplateName ASC";
                return GetTaskTemplates(SQL);
            }
        }

        public class ProjectTypes
        {
            /// <summary>
            /// Returns a ProjectTypeInfo instance with a given DataRow
            /// </summary>
            /// <param name="dr">The datarow</param>
            public static ProjectTypeInfo GetProjectType(DataRow dr)
            {
                ProjectTypeInfo info = new ProjectTypeInfo();

                info.ID = Convert.ToInt32(dr["ProjectTypeID"]);
                info.Order = Convert.ToInt32(dr["ProjectTypeOrder"]);
                info.Name = Convert.ToString(dr["ProjectTypeName"]);
                info.Code = Convert.ToString(dr["ProjectTypeCode"]);

                return info;
            }
            /// <summary>
            /// Gets dictionary of project types
            /// </summary>
            public static List<ProjectTypeInfo> GetProjectTypes()
            {
                List<ProjectTypeInfo> data = new List<ProjectTypeInfo>();

                string strSQL = "SELECT * " +
                     " FROM ProjectTypes " +
                     " ORDER BY ProjectTypeOrder";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetProjectType(dr));
                
                return data;
            }
        }

        public class ProjectGroups
        {
            /// <summary>
            /// Returns a ProjectGroupInfo instance with a given DataRow
            /// </summary>
            /// <param name="dr">The datarow</param>
            public static ProjectGroupInfo GetProjectGroup(DataRow dr)
            {
                ProjectGroupInfo info = new ProjectGroupInfo();
                info.ID = Convert.ToInt32(dr["ProjectGroupID"]);
                info.Name = Convert.ToString(dr["ProjectGroupName"]);
                info.Code = Convert.ToString(dr["ProjectGroupCode"]);
                if (dr["StartDate"] != DBNull.Value) info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                if (dr["EndDate"] != DBNull.Value) info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                info.ProjectTypeID = Convert.ToInt16(dr["ProjectTypeID"]);
                info.Status = (DTC.StatusEnum) Convert.ToInt16(dr["StatusID"]);
                if(dr["Details"] != DBNull.Value)
                    info.Details = Convert.ToString(dr["Details"]);

                return info;
            }
            public static ProjectGroupInfo GetProjectGroup(int id)
            {
                ProjectGroupInfo info = new ProjectGroupInfo();

                string strSQL = "SELECT * " +
                     " FROM ProjectGroups " +
                     " WHERE ProjectGroupID = " + id;
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    info = GetProjectGroup(dr);
                
                return info;
            }
            public static List<ProjectGroupInfo> GetProjectGroups()
            {
                string strSQL = "SELECT * " +
                     " FROM ProjectGroups " +
                     " ORDER BY ProjectGroupName";
                return GetProjectGroups(strSQL);
            }
            private static List<ProjectGroupInfo> GetProjectGroups(string strSQL)
            {
                List<ProjectGroupInfo> data = new List<ProjectGroupInfo>();
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetProjectGroup(dr));
                
                return data;
            }
            /// <summary>
            /// Gets dictionary of popular project groups
            /// </summary>
            /// <param name="n">TOP n</param>
            /// <param name="numDays">Number of days, history</param>
            /// <returns></returns>
            public static Dictionary<int, ProjectGroupInfo> GetPopularProjectGroups(int n, int numDays)
            {
                Dictionary<int, ProjectGroupInfo> dict = new Dictionary<int, ProjectGroupInfo>();

                string strSQL = "SELECT TOP (" + n + ") Tasks.ProjectGroupID," +
                    " SUM(Tasks.RealTime) AS TOTAL,  ProjectGroups.ProjectGroupCode," +
                    " ProjectGroups.ProjectGroupName,ProjectTypeID, ProjectGroups.IsActive," +
                    " ProjectGroups.IsActionable, ProjectGroups.IsCompletable, ProjectGroups.IsCompleted, " +
                    " ProjectGroups.StartDate, ProjectGroups.EndDate, ProjectGroups.DueDate, ProjectGroups.Details " +
                    " FROM Tasks INNER JOIN" +
                    " ProjectGroups ON Tasks.ProjectGroupID = ProjectGroups.ProjectGroupID" +
                    " WHERE ProjectGroups.IsActive = 1" +
                    " AND Tasks.TaskDate > " + DTC.Date.ObtainGoodDT(DateTime.Today.AddDays(-1 * numDays), true) +
                    " GROUP BY Tasks.ProjectGroupID, ProjectGroups.ProjectGroupCode," +
                    " ProjectGroups.ProjectGroupName,ProjectTypeID, ProjectGroups.IsActive," +
                    " ProjectGroups.IsActionable, ProjectGroups.IsCompletable, ProjectGroups.IsCompleted, ProjectGroups.StartDate," +
                    " ProjectGroups.EndDate, ProjectGroups.DueDate, ProjectGroups.Details" +
                    " ORDER BY ProjectGroups.ProjectGroupCode";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    dict.Add(Convert.ToInt32(dr["ProjectGroupID"]), GetProjectGroup(dr));
                
                return dict;
            }
            /// <summary>
            /// Adds a new ProjectGroup
            /// </summary>
            public static int AddUpdateProjectGroup(ProjectGroupInfo projectGroup)
            {
                string SQL = "";

                if (projectGroup.ID == 0)
                {
                    SQL = "SET NOCOUNT ON INSERT INTO ProjectGroups" +
                    " (ProjectGroupCode, ProjectGroupName, ProjectTypeID, " +
                    " StatusID, StartDate, EndDate, Details)" +
                    " VALUES (" +
                    "'" + DTC.Control.InputText(projectGroup.Code, 5) + "'," +
                    "'" + DTC.Control.InputText(projectGroup.Name, 25) + "'," +
                    projectGroup.ProjectTypeID + "," +
                    (int)projectGroup.Status + "," +
                    DTC.Date.ObtainGoodDT(projectGroup.StartDate, true) + "," +
                    DTC.Date.ObtainGoodDT(projectGroup.EndDate, true) + ", " +
                    "'" + DTC.Control.InputText(projectGroup.Details) + "'" +
                    ") SELECT SCOPE_IDENTITY() AS ProjectGroupID";
                    projectGroup.ID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE ProjectGroups SET" +
                    " ProjectGroupCode = '" + DTC.Control.InputText(projectGroup.Code, 5) + "'," +
                    " ProjectGroupName = '" + DTC.Control.InputText(projectGroup.Name, 25) + "'," +
                    " ProjectTypeID = " + projectGroup.ProjectTypeID + "," +
                    " StatusID = " + (int)projectGroup.Status + "," +
                    " StartDate = " + DTC.Date.ObtainGoodDT(projectGroup.StartDate, true) + "," +
                    " EndDate = " + DTC.Date.ObtainGoodDT(projectGroup.EndDate, true) + "," +
                    " Details = '" + DTC.Control.InputText(projectGroup.Details) + "'" +
                    " WHERE ProjectGroupID = " + projectGroup.ID;
                    RunNonQuery(SQL);
                }

                return projectGroup.ID;
            }
            public static string DeleteProjectGroup(int projectGroupID)
            {
                string strSQL = string.Empty;
                string tables = string.Empty;

                strSQL = "SELECT * FROM Projects " +
                    " WHERE ProjectGroupID = " + projectGroupID;
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Projects\n";

                strSQL = "SELECT * FROM Tasks " +
                    " WHERE ProjectGroupID = " + projectGroupID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Tasks\n";

                strSQL = "SELECT * FROM TaskTemplateLog " +
                    " WHERE ProjectGroupID = " + projectGroupID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Task Templates\n";

                strSQL = "SELECT * FROM CoTasks " +
                    " WHERE ProjectGroupID = " + projectGroupID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Co-Tasks\n";

                strSQL = "SELECT * FROM IdeaGroups " +
                    " WHERE ProjectGroupID = " + projectGroupID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Idea Groups\n";

                strSQL = "SELECT * FROM Pragmas " +
                    " WHERE ProjectGroupID = " + projectGroupID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Pragmas\n";

                strSQL = "SELECT * FROM PragmaInstances " +
                    " WHERE ProjectGroupID = " + projectGroupID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Pragma Instances\n";

                if (tables == string.Empty)
                {
                    strSQL = "DELETE ProjectGroups WHERE ProjectGroupID = " + projectGroupID;
                    RunNonQuery(strSQL);
                }
                
                return tables;
            }
        }

        public class Projects
        {
            public static int AddUpdateProject(ProjectInfo project)
            {
                string SQL = "";

                if (project.ID == 0)
                {
                    SQL = "SET NOCOUNT ON INSERT INTO Projects " +
                          " (ProjectCode, ProjectName, ProjectGroupID, ProjectImageThumb, " + 
                          " ProjectDetails, " +
                          " StatusID, RankID, IsCompletable, IsActionable, " +
                          " StartDate, EndDate, DueDate, TheOrder, ShowHowManyTasks, " +
                          " MonitoringFrequency, LatestBlockID, ProjectColor)" +
                          " VALUES (" +
                          "'" + DTC.Control.InputText(project.Code, 5) + "'," +
                          "'" + DTC.Control.InputText(project.Name, 25) + "'," +
                          project.ProjectGroupID + "," +
                          "'" + project.ProjectImageThumb + "'," +
                          "'" + DTC.Control.InputText(project.Details, 255) + "'," +
                          Convert.ToInt16(project.Status) + "," +
                          Convert.ToInt16(project.Rank) + "," +
                          Convert.ToInt16(project.IsCompletable) + "," +
                          Convert.ToInt16(project.IsActionable) + "," +
                          DTC.Date.ObtainGoodDT(project.StartDate, true) + "," +
                          DTC.Date.ObtainGoodDT(project.EndDate, true) + "," +
                          DTC.Date.ObtainGoodDT(project.DueDate, true) + "," +
                          project.Order + "," +
                          (int)project.ShowHowManyTasks + "," +
                          (int)project.MonitoringFrequency + "," +
                          project.LatestBlockID + "," +
                          "'" + project.ProjectColor + "'" +
                          ") SELECT SCOPE_IDENTITY() AS ProjectID";
                    project.ID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Projects SET " +
                        " ProjectCode = '" + DTC.Control.InputText(project.Code, 5) + "'," +
                        " ProjectName = '" + DTC.Control.InputText(project.Name, 25) + "'," +
                        " ProjectGroupID = " + project.ProjectGroupID + "," +
                        " ProjectImageThumb = '" + project.ProjectImageThumb + "', " +
                        " ProjectDetails = '" + DTC.Control.InputText(project.Details, 255) + "'," +
                        " StatusID = " + Convert.ToInt16(project.Status) + "," +
                        " RankID = " + Convert.ToInt16(project.Rank) + "," +
                        " IsCompletable = " + Convert.ToInt16(project.IsCompletable) + "," +
                        " IsActionable = " + Convert.ToInt16(project.IsActionable) + "," +
                        " StartDate = " + DTC.Date.ObtainGoodDT(project.StartDate, true) + "," +
                        " EndDate = " + DTC.Date.ObtainGoodDT(project.EndDate, true) + "," +
                        " DueDate = " + DTC.Date.ObtainGoodDT(project.DueDate, true) + "," +
                        " TheOrder = " + project.Order + "," +
                        " ShowHowManyTasks = " + (int)project.ShowHowManyTasks + "," +
                        " MonitoringFrequency = " + (int)project.MonitoringFrequency + ", " +
                        " LatestBlockID = " + project.LatestBlockID + "," +
                        " ProjectColor = '" + project.ProjectColor + "' " +
                        " WHERE ProjectID = " + project.ID;
                    RunNonQuery(SQL);
                }

                return project.ID;
            }
            private static ProjectInfo GetProject(DataRow dr)
            {
                ProjectInfo info = new ProjectInfo();

                info.ID = Convert.ToInt32(dr["ProjectID"]);
                info.Name = Convert.ToString(dr["ProjectName"]);
                info.Code = Convert.ToString(dr["ProjectCode"]);
                info.ProjectImgName = Convert.ToString(dr["ProjectImgName"]);
                info.ProjectImageThumb = Convert.ToString(dr["ProjectImageThumb"]);
                info.Details = Convert.ToString(dr["ProjectDetails"]);
                info.IsCompletable = Convert.ToBoolean(dr["IsCompletable"]);
                info.IsActionable = Convert.ToBoolean(dr["IsActionable"]);
                if (dr["StartDate"] != DBNull.Value) info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                if (dr["DueDate"] != DBNull.Value) info.DueDate = Convert.ToDateTime(dr["DueDate"]);
                if (dr["EndDate"] != DBNull.Value) info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                info.ProjectGroupID = Convert.ToInt16(dr["ProjectGroupID"]);
                info.Status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);
                info.Rank = (DTC.RankEnum)Convert.ToInt16(dr["RankID"]);
                info.Order = Convert.ToInt32(dr["TheOrder"]);
                info.CompletionRate = Convert.ToSingle(dr["CompletionRate"]);
                info.ShowHowManyTasks = (ProjectInfo.ShowHowManyTasksEnum)Convert.ToInt16(dr["ShowHowManyTasks"]);
                info.MonitoringFrequency = (DTC.RangeEnum)Convert.ToInt16(dr["MonitoringFrequency"]);
                info.LatestBlockID = Convert.ToInt32(dr["LatestBlockID"]);
                info.ProjectColor = Convert.ToString(dr["ProjectColor"]);

                return info;
            }
            public static List<ProjectInfo> GetActionableProjects()
            {
                List<ProjectInfo> data = new List<Models.ProjectInfo>();

                string strSQL = "SELECT * FROM Projects " +
                    " WHERE IsActionable = 1 " +
                    " AND StatusID = 1" +
                    " ORDER BY LastTaskDate DESC";
                DataTable dt = RunExecuteReader(strSQL);

                foreach (DataRow dr in dt.Rows)
                    data.Add(GetProject(dr));

                return data;
            }
            public static ProjectInfo GetProject(long projectID)
            {
                ProjectInfo info = new ProjectInfo();

                string strSQL = "SELECT * " +
                     " FROM Projects " +
                     " WHERE ProjectID = " + projectID;
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    info = GetProject(dr);

                return info;
            }
            public static int GetProjectXIcon(int projectID)
            {
                int xIConID = 0;

                string strSQL = "SELECT * " +
                     " FROM Projects " +
                     " WHERE ProjectID = " + projectID;
                DataTable dt = RunExecuteReader(strSQL);

                if (dt.Rows.Count == 1)
                    xIConID = Convert.ToInt32(dt.Rows[0]["XIconID"]);

                return xIConID;
            }
            public static List<ProjectInfo> GetProjects()
            {
                string strSQL = "SELECT * " +
                     " FROM Projects " +
                     " ORDER BY TheOrder";
                return GetProjects(strSQL);
            }
            public static List<ProjectInfo> GetProjects(int projectGroupID, DTC.StatusEnum status)
            {
                string strSQLStatus = string.Empty;
                if (status != DTC.StatusEnum.NA)
                    strSQLStatus = " AND StatusID = " + (int)status;

                string strSQL = "SELECT * FROM Projects" +
                    " WHERE ProjectGroupID = " + projectGroupID +
                    strSQLStatus +
                    " ORDER BY ProjectName";
                return GetProjects(strSQL);
            }
            public static List<ProjectInfo> GetProjectsCompleted(DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT * " +
                     " FROM Projects " +
                     " WHERE StatusID = 2" +
                     " AND EndDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                     " AND EndDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                     " ORDER BY EndDate DESC";
                return GetProjects(strSQL);
            }

            public static List<ProjectInfo> GetProjectsRelatedToTasks(DateTime dateStart, DateTime dateEnd)
            {
                string strSQL = "SELECT DISTINCT Projects.* " +
                    " FROM Tasks " +
                    " INNER JOIN " +
                    " Projects ON Tasks.ProjectID = Projects.ProjectID " +
                    " WHERE Tasks.TemplateID = 0" +
                    " AND Tasks.TaskDate >= " + DTC.ObtainGoodDT(dateStart, true) + 
                    " AND Tasks.TaskDate <= " + DTC.ObtainGoodDT(dateEnd, true);
                return GetProjects(strSQL);
            }

            public static List<ProjectSnapshotInfo> GetProjectsSnapshot()
            {
                List<ProjectSnapshotInfo> data = new List<ProjectSnapshotInfo>();
                DateTime dtTreshold = DateTime.Now.AddDays(-30);

                string strSQL = "SELECT Projects.ProjectID, Projects.StatusID, Projects.RankID, Projects.ProjectImageThumb, " + 
                    " Projects.ProjectName, Projects.ProjectCode,  Projects.ProjectDetails, " +
                    " SUM(Tasks.RealTime) AS RealTimeTotal, " +
                    " ProjectGroups.ProjectGroupCode, Projects.DueDate, Projects.StartDate, Projects.CompletionRate, " +
                    " Projects.EstimatedCompletionDate, Projects.EstimatedCompletionDateBasedOnLast30Days, Projects.HoursNeededToComplete, Projects.WorkPerDayNeededForDueDate, " +
                    " Projects.ProjectImgName, Projects.LastTaskDate, Projects.ProjectColor " +
                    " FROM Projects " +
                    " INNER JOIN Tasks ON Projects.ProjectID = Tasks.ProjectID " +
                    " INNER JOIN ProjectGroups ON Projects.ProjectGroupID = ProjectGroups.ProjectGroupID " +
                    " WHERE Projects.IsActionable = 1 " +
                    " AND Projects.StatusID = 1 " +
                    " AND Projects.RankID >= 0 " +
                    " AND Tasks.TaskDate >= " + DTC.ObtainGoodDT(dtTreshold, false) +
                    " GROUP BY Projects.ProjectID, Projects.StatusID, Projects.RankID, Projects.ProjectImageThumb, +" +
                    " Projects.ProjectName, Projects.ProjectCode, Projects.ProjectDetails," +
                    " ProjectGroups.ProjectGroupCode, Projects.DueDate, Projects.StartDate, Projects.CompletionRate, " +
                    " Projects.EstimatedCompletionDate, Projects.EstimatedCompletionDateBasedOnLast30Days, Projects.HoursNeededToComplete, Projects.WorkPerDayNeededForDueDate, " +
                    " Projects.ProjectImgName, Projects.LastTaskDate, Projects.ProjectColor " +
                    " ORDER BY Projects.LastTaskDate DESC";
                DataTable dt = RunExecuteReader(strSQL);

                foreach (DataRow dr in dt.Rows)
                {
                    DateTime dtStartDate = Convert.ToDateTime(dr["StartDate"]);
                    DateTime dtDueDate = DateTime.MaxValue;

                    if (dr["DueDate"] != DBNull.Value)
                        dtDueDate = Convert.ToDateTime(dr["DueDate"]);

                    ProjectSnapshotInfo projectSnapshot = new ProjectSnapshotInfo();
                    projectSnapshot.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                    projectSnapshot.Status = (DTC.StatusEnum) Convert.ToInt16(dr["StatusID"]);
                    projectSnapshot.Rank = (DTC.RankEnum)Convert.ToInt16(dr["RankID"]);
                    projectSnapshot.ProjectCode = Convert.ToString(dr["ProjectCode"]);
                    projectSnapshot.ProjectName = Convert.ToString(dr["ProjectName"]);
                    projectSnapshot.ProjectGroupCode = Convert.ToString(dr["ProjectGroupCode"]);
                    projectSnapshot.Details = Convert.ToString(dr["ProjectDetails"]);
                    projectSnapshot.RealTime = Convert.ToSingle(dr["RealTimeTotal"]);
                    projectSnapshot.DueDate = dtDueDate;
                    projectSnapshot.StartDate = dtStartDate;
                    if(dr["EstimatedCompletionDate"] != DBNull.Value)
                        projectSnapshot.EstimatedCompletionDate = Convert.ToDateTime(dr["EstimatedCompletionDate"]);
                    if (dr["EstimatedCompletionDateBasedOnLast30Days"] != DBNull.Value)
                        projectSnapshot.EstimatedCompletionDateBasedOnLast30Days = Convert.ToDateTime(dr["EstimatedCompletionDateBasedOnLast30Days"]);
                    projectSnapshot.CompletionRate = Convert.ToSingle(dr["CompletionRate"]);
                    projectSnapshot.HoursNeededToComplete = Convert.ToSingle(dr["HoursNeededToComplete"]);
                    projectSnapshot.WorkPerDayNeededForDueDate = Convert.ToSingle(dr["WorkPerDayNeededForDueDate"]);
                    projectSnapshot.ProjectImgName = Convert.ToString(dr["ProjectImgName"]);
                    projectSnapshot.ProjectColor = Convert.ToString(dr["ProjectColor"]);
                    projectSnapshot.PercentCompleted = 67;

                    if (dr["ProjectImageThumb"] != DBNull.Value)
                        projectSnapshot.ProjectImageThumb= Convert.ToString(dr["ProjectImageThumb"]);

                    projectSnapshot.NumDaysRemaining = 0;
                    if (dtDueDate > DateTime.Today && dtDueDate < DateTime.MaxValue)
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
                    dt = RunExecuteReader(strSQL);
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
                        ProjectSnapshotInfo ps = DB.Projects.GetProjectSnapshot(project.ID);
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

                string strSQL = "SELECT Projects.ProjectID, Projects.StatusID, Projects.RankID, Projects.ProjectImageThumb, " +
                    " Projects.ProjectName, Projects.ProjectCode, Projects.ProjectDetails, " +
                    " SUM(Tasks.RealTime) AS RealTimeTotal, ProjectGroups.ProjectGroupCode, " +
                    " Projects.DueDate, Projects.StartDate, " +
                    " Projects.EstimatedCompletionDate, Projects.EstimatedCompletionDateBasedOnLast30Days, Projects.HoursNeededToComplete, Projects.WorkPerDayNeededForDueDate," +
                    " Projects.ProjectImgName, Projects.ProjectColor " +
                    " FROM Projects " +
                    " INNER JOIN ProjectGroups ON Projects.ProjectGroupID = ProjectGroups.ProjectGroupID " +
                    " LEFT OUTER JOIN Tasks ON Projects.ProjectID = Tasks.ProjectID " +
                    " WHERE Projects.ProjectID = " + projectID +
                    " GROUP BY Projects.ProjectID, Projects.StatusID, Projects.RankID, Projects.ProjectImageThumb, " + 
                    " Projects.ProjectName, Projects.ProjectCode, Projects.ProjectDetails, " +
                    " ProjectGroups.ProjectGroupCode, Projects.DueDate, Projects.StartDate, " +
                    " Projects.EstimatedCompletionDate, Projects.EstimatedCompletionDateBasedOnLast30Days, Projects.HoursNeededToComplete, Projects.WorkPerDayNeededForDueDate, " +
                    " Projects.ProjectImgName, Projects.ProjectColor";
                DataTable dt = RunExecuteReader(strSQL);

                if (dt.Rows.Count == 1)
                {
                    DataRow dr = dt.Rows[0];
                    DateTime dtDueDate = Convert.ToDateTime(dr["DueDate"]);
                    DateTime dtStartDate = Convert.ToDateTime(dr["StartDate"]);

                    projectSnapshot.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                    projectSnapshot.Status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);
                    projectSnapshot.Rank = (DTC.RankEnum)Convert.ToInt16(dr["RankID"]);
                    projectSnapshot.ProjectCode = Convert.ToString(dr["ProjectCode"]);
                    projectSnapshot.ProjectName = Convert.ToString(dr["ProjectName"]);
                    projectSnapshot.ProjectGroupCode = Convert.ToString(dr["ProjectGroupCode"]);
                    projectSnapshot.Details = Convert.ToString(dr["ProjectDetails"]);
                    if (dr["RealTimeTotal"] != DBNull.Value)
                        projectSnapshot.RealTime = Convert.ToSingle(dr["RealTimeTotal"]);
                    projectSnapshot.StartDate = dtStartDate;
                    projectSnapshot.DueDate = dtDueDate;
                    if (dr["EstimatedCompletionDate"] != DBNull.Value)
                        projectSnapshot.EstimatedCompletionDate = Convert.ToDateTime(dr["EstimatedCompletionDate"]);
                    if (dr["EstimatedCompletionDateBasedOnLast30Days"] != DBNull.Value)
                        projectSnapshot.EstimatedCompletionDateBasedOnLast30Days = Convert.ToDateTime(dr["EstimatedCompletionDateBasedOnLast30Days"]);
                    projectSnapshot.ProjectImgName = Convert.ToString(dr["ProjectImgName"]);
                    projectSnapshot.ProjectColor = Convert.ToString(dr["ProjectColor"]);
                    projectSnapshot.PercentCompleted = GetProjectPerformance(projectID);
                    projectSnapshot.HoursNeededToComplete = Convert.ToSingle(dr["HoursNeededToComplete"]);
                    projectSnapshot.WorkPerDayNeededForDueDate = Convert.ToSingle(dr["WorkPerDayNeededForDueDate"]);

                    if (dr["ProjectImageThumb"] != DBNull.Value)
                        projectSnapshot.ProjectImageThumb = Convert.ToString(dr["ProjectImageThumb"]);

                    projectSnapshot.NumDaysRemaining = 0;
                    if (dtDueDate > DateTime.Today)
                        projectSnapshot.NumDaysRemaining = (int)dtDueDate.Subtract(DateTime.Today).TotalDays;
                }

                List<WeekInfo> weeks = new List<WeekInfo>();
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
                    dt = RunExecuteReader(strSQL);
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

                projectSnapshot.EternalTotalTime = GetProjectEternalTotalTime(projectID);

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
                DataTable dt = RunExecuteReader(strSQL);

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
                DataTable dt = RunExecuteReader(strSQL);

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
                DataTable dt = RunExecuteReader(strSQL);

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
            public static float GetProjectEternalTotalTime(long projectID)
            {
                float result = 0;

                List<long> list = new List<long>();
                list.Add(projectID);

                Dictionary<long, float> data = GetProjectEternalTotalTimes(list);
                if (data.ContainsKey(projectID))
                    result = data[projectID];

                return result;
            }
            public static List<ProjectInfo> GetProjects(string strSQL)
            {
                List<ProjectInfo> data = new List<ProjectInfo>();
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetProject(dr));

                return data;
            }
            public static void MoveProjectToAnotherGroup(ProjectInfo project, ProjectGroupInfo newPG)
            {
                string strSQL = "";

                strSQL = "UPDATE TaskTemplateLog " +
                    " SET ProjectGroupID = " + newPG.ID +
                    " WHERE ProjectID = " + project.ID;
                RunNonQuery(strSQL);

                strSQL = "UPDATE Tasks " +
                    " SET ProjectGroupID = " + newPG.ID +
                    " WHERE ProjectID = " + project.ID;
                RunNonQuery(strSQL);

                strSQL = "UPDATE CoTasks " +
                    " SET ProjectGroupID = " + newPG.ID +
                    " WHERE ProjectID = " + project.ID;
                RunNonQuery(strSQL);

                strSQL = "UPDATE IdeaGroups " +
                    " SET ProjectGroupID = " + newPG.ID +
                    " WHERE ProjectID = " + project.ID;
                RunNonQuery(strSQL);

                strSQL = "UPDATE Projects " +
                    " SET ProjectGroupID = " + newPG.ID +
                    " WHERE ProjectID = " + project.ID;
                RunNonQuery(strSQL);
            }
            public static void CreateGroupFromProject(ProjectInfo project)
            {
                ProjectGroupInfo newPG = new ProjectGroupInfo();

                newPG.Code = project.Code;
                newPG.EndDate = project.EndDate;
                newPG.StartDate = project.StartDate;
                newPG.Name = project.Name;
                newPG.Status = project.Status;
                newPG.ProjectTypeID = ProjectGroups.GetProjectGroup(project.ProjectGroupID).ProjectTypeID;

                newPG.ID = ProjectGroups.AddUpdateProjectGroup(newPG);

                MoveProjectToAnotherGroup(project, newPG);
            }
            public static List<ProjectInfo> GetImportantProjects(int n, bool isActionable, bool isCompletable)
            {
                string filterSQL = "";

                if (isActionable) filterSQL += " AND IsActionable = 1 ";
                if (isCompletable) filterSQL += " AND IsCompletable = 1 ";

                string strSQL = "SELECT TOP (" + n + ") * " +
                    " FROM Projects " +
                    " WHERE Projects.StatusID = 1" +
                    filterSQL +
                    " ORDER BY TheOrder";
                return GetProjects(strSQL);
            }
            public static string DeleteProject(long projectID)
            {
                string strSQL = "";
                string tables = "";

                strSQL = "SELECT * FROM Blocks " +
                    " WHERE ProjectID = " + projectID;
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Blocks\n";

                strSQL = "SELECT * FROM Tasks " +
                    " WHERE ProjectID = " + projectID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Tasks\n";

                strSQL = "SELECT * FROM TaskTemplateLog " +
                    " WHERE ProjectID = " + projectID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Task Templates\n";

                strSQL = "SELECT * FROM CoTasks " +
                    " WHERE ProjectID = " + projectID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Co-Tasks\n";

                strSQL = "SELECT * FROM IdeaGroups " +
                    " WHERE ProjectID = " + projectID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Idea Groups\n";

                strSQL = "SELECT * FROM Pragmas " +
                    " WHERE ProjectID = " + projectID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Pragmas\n";

                strSQL = "SELECT * FROM PragmaInstances " +
                    " WHERE ProjectID = " + projectID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    tables += "Pragma Instances\n";

                if (tables == "")
                {
                    strSQL = "DELETE Projects WHERE ProjectID = " + projectID;
                    RunNonQuery(strSQL);
                }
                
                return tables;
            }
            public static void ChangeProjectOrder(int projectID, int order)
            {
                string strSQL = "UPDATE Projects SET " +
                    " TheOrder = " + order +
                    " WHERE ProjectID = " + projectID;
                RunNonQuery(strSQL);
            }
            public static void InsertProjectOrder(List<ProjectInfo> projects, ProjectInfo project, int order)
            {
                foreach (ProjectInfo info in projects)
                {
                    if (info.Order >= order) info.Order++;
                    ChangeProjectOrder(info.ID, info.Order);
                }
                ChangeProjectOrder(project.ID, order);
            }
            public static Dictionary<int, DateTime> GetLastTaskDates(Dictionary<int, ProjectInfo> projects)
            {
                Dictionary<int, DateTime> dict = new Dictionary<int, DateTime>();
                string strSQL = "SELECT MAX(Tasks.TaskDate) AS LAST_DATE, Tasks.ProjectID, Projects.ProjectName" +
                    " FROM Tasks" +
                    " INNER JOIN Projects ON Tasks.ProjectID = Projects.ProjectID" +
                    " WHERE Tasks.IsCompleted = 1" +
                    " GROUP BY Tasks.ProjectID, Projects.ProjectName";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    int id = Convert.ToInt32(dr["ProjectID"]);
                    DateTime date = Convert.ToDateTime(dr["LAST_DATE"]);

                    if (!dict.ContainsKey(id))
                        dict.Add(id, date);
                }
                
                return dict;
            }
            public static Dictionary<int, float> GetTotalHours(Dictionary<int, ProjectInfo> projects)
            {
                Dictionary<int, float> dict = new Dictionary<int, float>();
                string strSQL = "SELECT CONVERT(float, SUM(RealTime)) / 60 AS TOTAL, ProjectID" +
                    " FROM Tasks" +
                    " WHERE ProjectID > 0 " +
                    " GROUP BY ProjectID";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    int id = Convert.ToInt32(dr["ProjectID"]);
                    float totalHours = Convert.ToSingle(dr["TOTAL"]);
                    if (!dict.ContainsKey(id))
                        dict.Add(id, totalHours);
                }
              
                return dict;
            }
            public static float GetTotalHours(ProjectInfo project)
            {
                float totalHours = 0;
                string strSQL = "SELECT CONVERT(float, SUM(RealTime)) / 60 AS TOTAL " +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + project.ID;
                DataTable dt = RunExecuteReader(strSQL);                
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    if (dr["TOTAL"] != DBNull.Value)
                        totalHours = Convert.ToSingle(dr["TOTAL"]);
                }
                
                return totalHours;
            }
            public static float GetTotalHoursLast30Days(ProjectInfo project)
            {
                float totalHours = 0;
                string strSQL = "SELECT CONVERT(float, SUM(RealTime)) / 60 AS TOTAL " +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + project.ID +
                    " AND TaskDate >= " + DTC.ObtainGoodDT(DateTime.Today.AddDays(-30) ,true);
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    if (dr["TOTAL"] != DBNull.Value)
                        totalHours = Convert.ToSingle(dr["TOTAL"]);
                }

                return totalHours;
            }
            public static Dictionary<int, ProjectPerfInfo> GetPerformanceFigures(List<ProjectInfo> projects, bool addToLog)
            {
                Dictionary<int, ProjectPerfInfo> dict = new Dictionary<int, ProjectPerfInfo>();
                Dictionary<int, Dictionary<int, SegmentPerfInfo>> segmentPerfs = Segments.GetSegmentPerfs(projects);

                foreach (ProjectInfo project in projects)
                {
                    if (!dict.ContainsKey(project.ID))
                        dict.Add(project.ID, new ProjectPerfInfo());

                    if (segmentPerfs.ContainsKey(project.ID))
                    {
                        foreach (SegmentPerfInfo sPerf in segmentPerfs[project.ID].Values)
                        {
                            dict[project.ID].Size += sPerf.Size;
                            dict[project.ID].NumTodos += sPerf.NumTodos;
                            dict[project.ID].NumCompleted += sPerf.NumCompleted;
                            dict[project.ID].CompletedSize += sPerf.GetCompletedSize();
                        }
                    }

                    if (addToLog)
                        AddProjectLog(project.ID, DateTime.Today, dict[project.ID].GetPerformance());
                }

                return dict;
            }
            public static ProjectPerfInfo GetPerformanceFigures(ProjectInfo project, bool addToLog)
            {
                ProjectPerfInfo pPerf = new ProjectPerfInfo();
                List<ProjectInfo> projects = new List<ProjectInfo>();
                projects.Add(project);

                Dictionary<int, ProjectPerfInfo> dict = GetPerformanceFigures(projects, addToLog);
                if (dict.ContainsKey(project.ID))
                    pPerf = dict[project.ID];

                return pPerf;
            }
            public static Dictionary<int, List<GoalInfo>> GetGoalsOfProjects(List<ProjectInfo> projects)
            {
                Dictionary<int, List<GoalInfo>> goalsOfProjects = new Dictionary<int, List<GoalInfo>>();

                string strSQL = "SELECT Projects.ProjectID, Projects.ProjectCode, Projects.ProjectName, Goals.* " +
                    " FROM Projects" +
                    " INNER JOIN Goals ON Projects.ProjectID = Goals.PrimaryProjectID" +
                    " WHERE Projects.IsActive = 1" +
                    " AND Goals.StatusID = 1" +
                    " AND Goals.GoalRange > 1" +
                    " ORDER BY Projects.ProjectID, Goals.GoalRange DESC";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    foreach (ProjectInfo project in projects)
                    {
                        int projectID = Convert.ToInt32(dr["ProjectID"]);
                        if (project.ID == projectID)
                        {
                            if (!goalsOfProjects.ContainsKey(projectID))
                                goalsOfProjects.Add(projectID, new List<GoalInfo>());
                            GoalInfo goal = DB.Goals.GetGoal(dr, true);
                            goalsOfProjects[projectID].Add(goal);
                        }
                    }
                }
                
                return goalsOfProjects;
            }
            public static Dictionary<int, float> GetYearlyHoursOfProject(long projectID)
            {
                Dictionary<int, float> dict = new Dictionary<int, float>();
                string strSQL = string.Empty;

                DateTime minDate = DateTime.Today;
                DateTime maxDate = DateTime.Today;

                strSQL = "SELECT MIN(TaskDate) AS MinDate, MAX(TaskDate) AS MaxDate" +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + projectID +
                    " AND IsCompleted = 1 AND IsActive = 1";
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    if (dr["MinDate"] != DBNull.Value)
                        minDate = Convert.ToDateTime(dr["MinDate"]);
                    if (dr["MaxDate"] != DBNull.Value)
                        maxDate = Convert.ToDateTime(dr["MaxDate"]);
                }

                bool timelineCompleted = false;
                int year = minDate.Year;
                while (!timelineCompleted)
                {
                    if (!dict.ContainsKey(year))
                        dict.Add(year, 0);
                     
                    if (year >= maxDate.Year)
                    {
                        timelineCompleted = true;
                    }
                    else
                    {
                        year++;
                    }
                }

                strSQL = "SELECT CONVERT(FLOAT, SUM(RealTime)) / 60 AS Total, " +
                    " YEAR(TaskDate) AS TheYear" +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + projectID +
                    " AND IsCompleted = 1 AND IsActive = 1" +
                    " GROUP BY YEAR(TaskDate) " +
                    " ORDER BY TheYear";
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    year = Convert.ToInt16(dr["TheYear"]);
                    if (dict.ContainsKey(year))
                    {
                        dict[year] = Convert.ToSingle(dr["Total"]);
                    }
                }

                return dict;
            }
            public static Dictionary<int, Dictionary<int, float>> GetMonthlyHoursOfProject(ProjectInfo project)
            {
                Dictionary<int, Dictionary<int, float>> dict = new Dictionary<int, Dictionary<int, float>>();
                string strSQL = string.Empty;

                DateTime minDate = DateTime.Today;
                DateTime maxDate = DateTime.Today;

                strSQL = "SELECT MIN(TaskDate) AS MinDate, MAX(TaskDate) AS MaxDate" +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + project.ID +
                    " AND IsCompleted = 1 AND IsActive = 1";
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    if (dr["MinDate"] != DBNull.Value)
                        minDate = Convert.ToDateTime(dr["MinDate"]);
                    if (dr["MaxDate"] != DBNull.Value)
                        maxDate = Convert.ToDateTime(dr["MaxDate"]);
                }
                 
                bool timelineCompleted = false;
                int month = minDate.Month; ;
                int year = minDate.Year;
                while (!timelineCompleted)
                {
                    if (!dict.ContainsKey(year))
                        dict.Add(year, new Dictionary<int, float>());
                    if (!dict[year].ContainsKey(month))
                        dict[year].Add(month, 0);

                    if (year >= maxDate.Year && month >= maxDate.Month)
                    {
                        timelineCompleted = true;
                    }
                    else
                    {
                        month++;
                        if (month > 12)
                        {
                            month = 1;
                            year++;
                        }
                    }
                }

                strSQL = "SELECT CONVERT(FLOAT, SUM(RealTime)) / 60 AS Total, " +
                    " MONTH(TaskDate) AS TheMonth, YEAR(TaskDate) AS TheYear" +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + project.ID +
                    " AND IsCompleted = 1 AND IsActive = 1" +
                    " GROUP BY YEAR(TaskDate), MONTH(TaskDate)" +
                    " ORDER BY TheYear, TheMonth";
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    month = Convert.ToInt16(dr["TheMonth"]);
                    year = Convert.ToInt16(dr["TheYear"]);
                    if (dict.ContainsKey(year))
                    {
                        if (dict[year].ContainsKey(month))
                            dict[year][month] = Convert.ToSingle(dr["Total"]);
                    }
                }
                
                return dict;
            }
            public static Dictionary<int, Dictionary<int, float>> GetWeeklyHoursOfProject(ProjectInfo project)
            {
                Dictionary<int, Dictionary<int, float>> dict = new Dictionary<int, Dictionary<int, float>>();
                string strSQL = string.Empty;

                DateTime minDate = DateTime.Today;
                DateTime maxDate = DateTime.Today;

                strSQL = "SELECT MIN(TaskDate) AS MinDate, MAX(TaskDate) AS MaxDate" +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + project.ID +
                    " AND IsCompleted = 1 AND IsActive = 1";
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    if (dr["MinDate"] != DBNull.Value)
                        minDate = Convert.ToDateTime(dr["MinDate"]);
                    if (dr["MaxDate"] != DBNull.Value)
                        maxDate = Convert.ToDateTime(dr["MaxDate"]);
                }
                 
                bool timelineCompleted = false;
                int week = DTC.Date.GetWeekNumber(minDate);
                int year = minDate.Year;
                while (!timelineCompleted)
                {
                    if (!dict.ContainsKey(year))
                        dict.Add(year, new Dictionary<int, float>());
                    if (!dict[year].ContainsKey(week))
                        dict[year].Add(week, 0);

                    if (year >= maxDate.Year && week >= DTC.Date.GetWeekNumber(maxDate))
                    {
                        timelineCompleted = true;
                    }
                    else
                    {
                        week++;
                        if (week > 52)
                        {
                            week = 1;
                            year++;
                        }
                    }
                }

                strSQL = "SELECT CONVERT(FLOAT, SUM(RealTime)) / 60 AS Total, " +
                    "{ fn WEEK(TaskDate) } AS TheWeek, YEAR(TaskDate) AS TheYear" +
                    " FROM Tasks" +
                    " WHERE ProjectID = " + project.ID +
                    " AND (IsCompleted = 1) AND (IsActive = 1)" +
                    " GROUP BY YEAR(TaskDate), { fn WEEK(TaskDate) }" +
                    " ORDER BY TheYear, TheWeek";

                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    week = Convert.ToInt16(dr["TheWeek"]);
                    year = Convert.ToInt16(dr["TheYear"]);
                    if (dict.ContainsKey(year))
                    {
                        if (!dict[year].ContainsKey(week)) dict[year].Add(week, 0);
                        if (dict[year].ContainsKey(week))
                        {
                            dict[year][week] = Convert.ToSingle(dr["Total"]);
                        }
                    }
                }
                 
                return dict;
            }
            public static int AddProjectLog(int projectID, DateTime date, float performance)
            {
                string sql;
                sql = "DELETE ProjectLogNG " +
                    " WHERE ProjectID = " + projectID +
                    " AND TheDate = " + DTC.Date.ObtainGoodDT(date, true);
                RunNonQuery(sql);

                sql = "INSERT INTO ProjectLogNG " +
                    " (ProjectID, TheDate, Performance) VALUES (" +
                    projectID + "," +
                    DTC.Date.ObtainGoodDT(date, true) + "," +
                    DTC.Control.CommaToDot(performance) +
                    ")  SELECT SCOPE_IDENTITY() AS ID";
                return (int)RunExecuteScalar(sql);
            }
            public static List<Tuple<int, int, float>> GetWeeklyAverageLogs(int projectID, int nTop)
            {
                List<Tuple<int, int, float>> dataTemp = new List<Tuple<int, int, float>>();
                List<Tuple<int, int, float>> data = new List<Tuple<int, int, float>>();
                

                string strSQL = "SELECT { fn WEEK(TheDate) } AS THE_WEEK, { fn YEAR(TheDate) } AS THE_YEAR, AVG(Performance) AS THE_AVERAGE " +
                    " FROM ProjectLogNG " +
                    " WHERE ProjectID = " + projectID +
                    " GROUP BY { fn WEEK(TheDate) }, { fn YEAR(TheDate) } " +
                    " ORDER BY THE_YEAR ASC, THE_WEEK ASC";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    dataTemp.Add(new Tuple<int, int, float>(Convert.ToInt16(dr["THE_YEAR"]), Convert.ToInt16(dr["THE_WEEK"]), Convert.ToSingle(dr["THE_AVERAGE"])));
                }

                int counter = 0;
                for (int i=dataTemp.Count - 1;i>=0;i--)
                {
                    if (counter < nTop)
                        data.Add(dataTemp[i]);
                    else
                        break;

                    counter++;
                }
                data.Reverse();

                return data;
            }
            public static float GetDailyProjectLog(ProjectInfo project, DateTime date)
            {
                float performance = 0;

                string strSQL = "SELECT * FROM ProjectLogNG " +
                    " WHERE ProjectID = " + project.ID +
                    " AND TheDate = " + DTC.Date.ObtainGoodDT(date, true);
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    performance = Convert.ToSingle(dr["Performance"]);
                }

                return performance;
            }
            public static Dictionary<int, float> GetWeeklyLogsOfProjects(DateTime date)
            {
                Dictionary<int, float> dict = new Dictionary<int, float>();
                WeekInfo week = Weeks.GetWeek(date, false);
                string strSQL = "SELECT AVG(Performance) AS AVG_PERF, ProjectID" +
                    " FROM ProjectLogNG " +
                    " WHERE TheDate >= " + DTC.Date.ObtainGoodDT(week.StartDate, true) +
                    " AND TheDate <= " + DTC.Date.ObtainGoodDT(week.EndDate, true) +
                    " AND (Performance > 0)" +
                    " GROUP BY ProjectID" +
                    " ORDER BY ProjectID";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    float performance = Convert.ToSingle(dr["AVG_PERF"]);
                    int projectID = Convert.ToInt32(dr["ProjectID"]);
                    if (!dict.ContainsKey(projectID))
                    {
                        dict.Add(projectID, performance);
                    }
                }
                 
                return dict;
            }
            public static Dictionary<int, float> GetMonthlyLogsOfProjects(DateTime date)
            {
                Dictionary<int, float> dict = new Dictionary<int, float>();
                MonthInfo month = Months.GetMonth(date, false);
                string strSQL = "SELECT AVG(Performance) AS AVG_PERF, ProjectID" +
                    " FROM ProjectLogNG " +
                    " WHERE TheDate >= " + DTC.Date.ObtainGoodDT(month.StartDate, true) +
                    " AND TheDate <= " + DTC.Date.ObtainGoodDT(month.EndDate, true) +
                    " AND (Performance > 0)" +
                    " GROUP BY ProjectID" +
                    " ORDER BY ProjectID";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    float performance = Convert.ToSingle(dr["AVG_PERF"]);
                    int projectID = Convert.ToInt32(dr["ProjectID"]);
                    if (!dict.ContainsKey(projectID))
                    {
                        dict.Add(projectID, performance);
                    }
                }
                
                return dict;
            }
            public static DateTime GetEstimatedCompletionDate(int projectID)
            {
                DateTime result = DateTime.MaxValue;
                ProjectInfo project = GetProject(projectID);
                float daysSpent = (float)DateTime.Today.Subtract(project.StartDate).TotalDays;

                float percentComplete = 0;
                List<ClusterInfo> clusters = Clusters.GetClustersOfProject(projectID, true);
                foreach(ClusterInfo cluster in clusters)
                    percentComplete += cluster.Contribution;

                if(percentComplete > 0)
                {
                    float totalDaysNeeded = daysSpent / percentComplete;
                    result = project.StartDate.AddDays(totalDaysNeeded);
                }
                
                return result;
            }
            public static void UpdateCompletionRateAndHoursNeeded(int projectID)
            {
                ProjectInfo project = GetProject(projectID);
                float daysSpent = (float)DateTime.Today.Subtract(project.StartDate).TotalDays;

                float completionRate = 0;
                float hoursNeededToComplete = 0;
                List<ClusterInfo> clusters = Clusters.GetClustersOfProject(projectID, true);
                foreach (ClusterInfo cluster in clusters)
                {
                    completionRate += cluster.Contribution;
                    if(!cluster.IsCompleted)
                    {
                        float difference = cluster.HoursNeeded - cluster.HoursSpent;
                        if (difference > 0)
                            hoursNeededToComplete += difference;
                    }
                }
                    
                DateTime estimatedCompletionDate = DateTime.MaxValue;
                DateTime estimatedCompletionDateBasedOnLast30Days = DateTime.MaxValue;
                float workPerDayNeededForDueDate = 0;

                float daysLeftForDueDate = (float)project.DueDate.Subtract(DateTime.Today).TotalDays;
                if(daysLeftForDueDate > 0)
                {
                    workPerDayNeededForDueDate = hoursNeededToComplete / daysLeftForDueDate;
                }

                float taskHoursTotal = GetTotalHours(project);
                float taskHoursLast30Days = GetTotalHoursLast30Days(project);

                if (completionRate > 0)
                {
                    float totalDaysNeeded = daysSpent / completionRate;
                    float totalHoursNeededToCompleteTheProject = taskHoursTotal / completionRate;
                    float hoursLeft = totalHoursNeededToCompleteTheProject - taskHoursTotal;

                    //float totalDaysNeededBasedOnLast30Days = totalDaysNeeded * (taskHoursTotal / daysSpent) / (taskHoursLast30Days / 30);
                    float totalDaysNeededBasedOnLast30Days = 10 * 365;  // if not worked within 30 days, declare it as 10 years
                    if(taskHoursLast30Days > 0)
                        totalDaysNeededBasedOnLast30Days = hoursLeft / (taskHoursLast30Days / 30);

                    estimatedCompletionDate = project.StartDate.AddDays(totalDaysNeeded);
                    estimatedCompletionDateBasedOnLast30Days = DateTime.Today.AddDays(totalDaysNeededBasedOnLast30Days);
                }
                
                string strSQL = "UPDATE Projects SET" +
                    " CompletionRate = " + completionRate + "," +
                    " HoursNeededToComplete = " + hoursNeededToComplete + "," +
                    " EstimatedCompletionDate = " + DTC.ObtainGoodDT(estimatedCompletionDate, true) + "," +
                    " EstimatedCompletionDateBasedOnLast30Days = " + DTC.ObtainGoodDT(estimatedCompletionDateBasedOnLast30Days, true) + "," +
                    " WorkPerDayNeededForDueDate = " + workPerDayNeededForDueDate +
                    " WHERE ProjectID = " + projectID;
                RunNonQuery(strSQL);

                AddProjectLog(projectID, DateTime.Today, completionRate);
            }

            public static List<ProjectMonitorItemInfo> GetProjectsMonitorValues()
            {
                List<ProjectMonitorItemInfo> data = new List<ProjectMonitorItemInfo>();
                DateTime dtStart = new DateTime(2023, 1, 1);
                DateTime dtEnd = new DateTime(2023, 1, 31);

                string strSQL = "SELECT Projects.ProjectID, Tasks.TaskDate, SUM(Tasks.RealTime) AS NUMMINUTES" + 
                    " FROM Projects INNER JOIN Tasks ON Projects.ProjectID = Tasks.ProjectID" +
                    " WHERE (Projects.ProjectID = 521) " +
                    " AND Tasks.TaskDate >= " + DTC.Date.ObtainGoodDT(dtStart, true) + "" +
                    " AND Tasks.TaskDate <= " + DTC.Date.ObtainGoodDT(dtEnd, true) + "" +
                    " GROUP BY Projects.ProjectID, Tasks.TaskDate " +
                    " ORDER BY Tasks.TaskDate";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    int numMinutes = Convert.ToInt16(dr["NUMMINUTES"]);
                    int projectID = Convert.ToInt32(dr["ProjectID"]);
                    DateTime theDate = Convert.ToDateTime(dr["TaskDate"]);

                    data.Add(new ProjectMonitorItemInfo() { NumMinutes = numMinutes, ProjectID = projectID, TheDate = theDate });
                }

                return data;
            }
            public static bool UpdateLatestBlockD(long projectID, long latestBlockD)
            {
                string strSQL = "UPDATE Projects SET " +
                    " LatestBlockID = " + latestBlockD +
                    " WHERE ProjectID = " + projectID;
                RunNonQuery(strSQL);

                return true;
            }
        }

        public class Clusters
        {
            public static ClusterInfo GetCluster(DataRow dr)
            {
                ClusterInfo info = new ClusterInfo();

                info.ID = Convert.ToInt32(dr["ClusterID"]);
                info.SubProjectID = Convert.ToInt32(dr["SubProjectID"]);
                info.Title = Convert.ToString(dr["ClusterTitle"]);
                info.HoursNeeded = Convert.ToSingle(dr["HoursNeeded"]);
                info.IsCompleted = Convert.ToBoolean(dr["IsCompleted"]);
                info.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                if (dr["StartDate"] != DBNull.Value) info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                if (dr["EndDate"] != DBNull.Value) info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                if (dr["Details"] != DBNull.Value) info.Details = Convert.ToString(dr["Details"]);

                return info;
            }
            public static ClusterInfo GetCluster(long clusterID)
            {
                ClusterInfo info = new ClusterInfo();

                string strSQL = "SELECT * FROM Clusters " +
                    " WHERE ClusterID = " + clusterID;
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                    info = GetCluster(dt.Rows[0]);

                return info;
            }
            public static List<ClusterInfo> GetClustersOfSubProject(int subProjectID, bool computeAllParameters)
            {
                List<ClusterInfo> data = new List<ClusterInfo>();

                string strSQL = "SELECT * FROM Clusters " +
                    " WHERE SubProjectID = " + subProjectID +
                    " ORDER BY ClusterTitle";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetCluster(dr));

                if (computeAllParameters)
                    ComputeAllPArameters(data);

                return data;
            }
            public static List<ClusterInfo> GetClustersOfProject(int projectID, bool computeAllParameters)
            {
                List<ClusterInfo> data = new List<ClusterInfo>();

                string strSQL = "SELECT * FROM Clusters " +
                    " WHERE ProjectID = " + projectID +
                    " ORDER BY ClusterTitle";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetCluster(dr));

                if (computeAllParameters)
                   ComputeAllPArameters(data);

                return data;
            }
            public static long AddUpdateCluster(ClusterInfo cluster)
            {
                string SQL = "";

                if (cluster.ID == 0)
                {
                    SQL = "SET NOCOUNT ON INSERT INTO Clusters " +
                         " (ClusterTitle, HoursNeeded, IsCompleted, ProjectID, SubProjectID," +
                         " StartDate, EndDate, Details " +
                         " )" +
                         " VALUES (" +
                         "'" + DTC.Control.InputText(cluster.Title, 50) + "'," +
                         cluster.HoursNeeded + "," +
                         Convert.ToInt32(cluster.IsCompleted) + "," +
                         cluster.ProjectID + "," +
                         cluster.SubProjectID + "," +
                         DTC.Date.ObtainGoodDT(cluster.StartDate, true) + "," +
                         DTC.Date.ObtainGoodDT(cluster.EndDate, true) + "," +
                         "'" + DTC.Control.InputText(cluster.Details, 255) + "'" +
                         ") SELECT SCOPE_IDENTITY() AS ClusterID";
                    cluster.ID = RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Clusters SET " +
                       " ClusterTitle = '" + DTC.Control.InputText(cluster.Title, 255) + "'," +
                       " HoursNeeded = " + cluster.HoursNeeded + "," +
                       " IsCompleted = " + Convert.ToInt16(cluster.IsCompleted) + "," +
                       " ProjectID = " + cluster.ProjectID + "," +
                       " SubProjectID = " + cluster.SubProjectID + "," +
                       " StartDate = " + DTC.Date.ObtainGoodDT(cluster.StartDate, true) + "," +
                       " EndDate = " + DTC.Date.ObtainGoodDT(cluster.EndDate, true) + "," +
                       " Details = '" + DTC.Control.InputText(cluster.Details, 999) + "'" +
                       " WHERE ClusterID = " + cluster.ID;
                    RunNonQuery(SQL);
                }

                return cluster.ID;
            }
            public static bool DeleteCluster(long clusterID)
            {
                bool isOK = true;

                string strSQL = "SELECT * FROM Blocks " +
                    " WHERE ClusterID = " + clusterID;
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                    isOK = false;

                if (isOK)
                {
                    strSQL = "DELETE Clusters WHERE ClusterID = " + clusterID;
                    RunNonQuery(strSQL);
                }

                return isOK;
            }

            private static void ComputeAllPArameters(List<ClusterInfo> data)
            {
                int projectID = 0;
                if (data.Count > 0)
                    projectID = data.First().ProjectID;

                List<BlockInfo> blocks = Blocks.GetBlocks(projectID);
                string strSQL = "SELECT Clusters.ClusterID, SUM(Tasks.RealTime) AS TotalMinutes " +
                    " FROM Clusters " +
                    " INNER JOIN Blocks ON Clusters.ClusterID = Blocks.ClusterID " +
                    " INNER JOIN Segments ON Blocks.BlockID = Segments.BlockID " +
                    " INNER JOIN Tasks ON Segments.SegmentID = Tasks.SegmentID " +
                    " WHERE Clusters.ProjectID = " + projectID +
                    " GROUP BY Clusters.ClusterID";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    long clusterID = Convert.ToInt32(dr["ClusterID"]);
                    ClusterInfo cluster = data.Find(i => i.ID == clusterID);
                    if (cluster != null)
                    {
                        List<BlockInfo> blocksOfCluster = blocks.FindAll(q => q.ClusterID == cluster.ID);

                        cluster.NumBlocksTotal = 0;
                        cluster.NumBlocksCompleted = 0;

                        if (blocksOfCluster != null)
                        {
                            cluster.NumBlocksTotal = blocksOfCluster.Count;
                            cluster.NumBlocksCompleted = blocksOfCluster.FindAll(q => q.Status != DTC.StatusEnum.Running).Count;
                        }

                        float hoursSpent = Convert.ToSingle(dr["TotalMinutes"]) / 60f;
                        cluster.HoursSpent = hoursSpent;
                        cluster.RatioSpentNeeded = cluster.GetRatioSpentNeeded();
                    }
                }

                float overall = data.Sum(i => i.GetEffectiveHoursNeeded());

                if (overall > 0)
                {
                    foreach (ClusterInfo cluster in data)
                    {
                        cluster.Contribution = cluster.RatioSpentNeeded * (cluster.GetEffectiveHoursNeeded() / overall);
                        cluster.ContributionMax = cluster.GetEffectiveHoursNeeded() / overall;
                    }
                }
            }
        }

        public class SubProjects
        {
            private static SubProjectInfo GetSubProject(DataRow dr)
            {
                SubProjectInfo info = new SubProjectInfo();

                info.ID = Convert.ToInt32(dr["SubProjectID"]);
                info.Name = Convert.ToString(dr["SubProjectName"]);
                info.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                info.IsCompleted = Convert.ToBoolean(dr["IsCompleted"]);
                if (dr["StartDate"] != DBNull.Value) info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                if (dr["EndDate"] != DBNull.Value) info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                if (dr["Details"] != DBNull.Value) info.Details = Convert.ToString(dr["Details"]);

                return info;
            }
            public static SubProjectInfo GetSubProject(long subProjectID)
            {
                SubProjectInfo info = new SubProjectInfo();

                string strSQL = "SELECT * FROM SubProjects " +
                    " WHERE SubProjectID = " + subProjectID;
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                    info = GetSubProject(dt.Rows[0]);

                return info;
            }
            public static List<SubProjectInfo> GetSubProjects(int projectID, bool isOnlyRunning)
            {
                List<SubProjectInfo> data = new List<SubProjectInfo>();

                string strFilter = string.Empty;

                if(isOnlyRunning)
                    strFilter = " AND IsCompleted = 0 ";

                string strSQL = "SELECT * FROM SubProjects " +
                    " WHERE ProjectID = " + projectID +
                    strFilter +
                    " ORDER BY SubProjectOrder";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetSubProject(dr));
             
                return data;
            }
            public static List<SubProjectInfo> GetSubProjects(int projectID, bool isOnlyRunning, bool isCalculatePerformance)
            {
                List<SubProjectInfo> subProjects = GetSubProjects(projectID, isOnlyRunning);

                if(isCalculatePerformance)
                {
                    List<ClusterInfo> clusters = DB.Clusters.GetClustersOfProject(projectID, true);

                    foreach(ClusterInfo cluster in clusters)
                    {
                        SubProjectInfo subProject = subProjects.Find(q=>q.ID == cluster.SubProjectID);
                        if(subProject != null)
                        {
                            subProject.NumClustersTotal++;
                            subProject.HoursNeeded += cluster.HoursNeeded;
                            subProject.HoursSpent += cluster.HoursSpent;
                            subProject.RatioSpentNeeded += cluster.RatioSpentNeeded;
                            subProject.Contribution += cluster.Contribution;
                            subProject.ContributionMax += cluster.ContributionMax;
                        }
                    }

                    foreach(SubProjectInfo sP in subProjects)
                    {
                        if(sP.ContributionMax > 0)
                        {
                            sP.ContributionPercentage = 100f * sP.Contribution / sP.ContributionMax;
                            if (sP.ContributionPercentage > 100)
                                sP.ContributionPercentage = 100;
                        }
                        else
                        {
                            sP.ContributionPercentage = 0;
                        }
                    }
                }

                return subProjects;
            }
            public static long AddUpdateSubProject(SubProjectInfo subProject)
            {
                string strSQL;

                if (subProject.ID == 0)
                {
                    strSQL = "SET NOCOUNT ON INSERT INTO SubProjects " +
                         " (SubProjectName, ProjectID, IsCompleted, " +
                         " StartDate, EndDate, Details " +
                         " )" +
                         " VALUES (" +
                         "'" + DTC.Control.InputText(subProject.Name, 50) + "'," +
                         subProject.ProjectID + "," +
                         Convert.ToInt32(subProject.IsCompleted) + "," +
                         DTC.Date.ObtainGoodDT(subProject.StartDate, true) + "," +
                         DTC.Date.ObtainGoodDT(subProject.EndDate, true) + "," +
                         "'" + DTC.Control.InputText(subProject.Details, 255) + "'" +
                         ") SELECT SCOPE_IDENTITY() AS SubProjectID";
                    subProject.ID = RunExecuteScalar(strSQL);
                }
                else
                {
                    strSQL = "UPDATE SubProjects SET " +
                       " SubProjectName = '" + DTC.Control.InputText(subProject.Name, 255) + "'," +
                       " ProjectID = " + subProject.ProjectID + "," +
                       " IsCompleted = " + Convert.ToInt16(subProject.IsCompleted) + "," +
                       " StartDate = " + DTC.Date.ObtainGoodDT(subProject.StartDate, true) + "," +
                       " EndDate = " + DTC.Date.ObtainGoodDT(subProject.EndDate, true) + "," +
                       " Details = '" + DTC.Control.InputText(subProject.Details, 999) + "'" +
                       " WHERE SubProjectID = " + subProject.ID;
                    RunNonQuery(strSQL);
                }

                return subProject.ID;
            }
            public static bool DeleteSubProject(long subProjectID)
            {
                bool isOK = true;

                string strSQL = "SELECT * FROM Clusters " +
                    " WHERE SubProjectID = " + subProjectID;
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                    isOK = false;

                if (isOK)
                {
                    strSQL = "DELETE SubProjects WHERE SubProjectID = " + subProjectID;
                    RunNonQuery(strSQL);
                }

                return isOK;
            }
            public static SubProjectPerformanceInfo GetSubProjectPerformance(long subProjectID)
            {
                SubProjectPerformanceInfo info = new SubProjectPerformanceInfo();

                string strSQL = "SELECT SUM(Tasks.RealTime) AS TotalMinutes, " +
                    " Clusters.ClusterID, Clusters.ClusterTitle, Clusters.HoursNeeded, Clusters.IsCompleted " +
                    " FROM SubProjects " +
                    " INNER JOIN Clusters ON SubProjects.SubProjectID = Clusters.SubProjectID " +
                    " LEFT OUTER JOIN Tasks " +
                    " INNER JOIN Blocks ON Tasks.BlockID = Blocks.BlockID ON Clusters.ClusterID = Blocks.ClusterID " +
                    " WHERE SubProjects.SubProjectID = " + subProjectID +
                    " GROUP BY Clusters.ClusterID, Clusters.ClusterTitle, Clusters.HoursNeeded, Clusters.IsCompleted " +
                    " ORDER BY Clusters.ClusterTitle";
                DataTable dt = RunExecuteReader(strSQL);

                foreach (DataRow dr in dt.Rows)
                {
                    info.NumClustersTotal++;
                    if (Convert.ToBoolean(dr["IsCompleted"]))
                        info.NumClustersCompleted++;
                    info.HoursNeeded += Convert.ToSingle(dr["HoursNeeded"]);

                    float hoursSpent = 0;
                    if(dr["TotalMinutes"] != DBNull.Value)
                    {
                        hoursSpent = Convert.ToSingle(dr["TotalMinutes"]) / 60f;
                    }
                    
                    info.HoursSpentInRealLife += hoursSpent;
                    if (hoursSpent > info.HoursNeeded)
                        hoursSpent = info.HoursNeeded;
                    info.HoursSpent+=  hoursSpent;
                }

                info.PercentageCompleted = 100f * info.HoursSpent / info.HoursNeeded;

                /*
                strSQL = "SELECT Tasks.BlockID, Tasks.RealTime, Blocks.Title, Clusters.ClusterID, Clusters.ClusterTitle, " +
                    " SubProjects.SubProjectID, SubProjects.SubProjectName" + 
                    " FROM Tasks " +
                    " INNER JOIN Blocks ON Tasks.BlockID = Blocks.BlockID " + 
                    " INNER JOIN Clusters ON Blocks.ClusterID = Clusters.ClusterID " +
                    " INNER JOIN SubProjects ON Clusters.SubProjectID = SubProjects.SubProjectID " +
                    " WHERE SubProjects.SubProjectID = " + subProjectID;
                dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    info.HoursSpentInRealLife+= Convert.ToSingle(dr["RealTime"]) / 60f;
                }
                */


                return info;
            }

        }

        public class Blocks
        {
            public static BlockInfo GetBlockWithGoal(DataRow dr)
            {
                BlockInfo info = new BlockInfo();

                info.ID = Convert.ToInt32(dr["BlockID"]);
                info.Title = Convert.ToString(dr["Title"]);
                info.HasDue = Convert.ToBoolean(dr["HasDue"]);
                info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                if (dr["EndDate"] != DBNull.Value) info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                if (dr["DueDate"] != DBNull.Value) info.DueDate = Convert.ToDateTime(dr["DueDate"]);
                if (Convert.ToInt32(dr["ProjectID"]) > 0)
                    info.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                info.ClusterID = Convert.ToInt32(dr["ClusterID"]);
                if (dr["Details"] != DBNull.Value) info.Details = Convert.ToString(dr["Details"]);
                info.Status = (DTC.StatusEnum)Convert.ToInt32(dr["StatusID"]);
                info.Order = Convert.ToInt32(dr["TheOrder"]);
                if (dr["ProjectCode"] != DBNull.Value) info.ProjectCode = Convert.ToString(dr["ProjectCode"]);
                if (dr["GoalID"] != DBNull.Value) info.RunningGoalID = Convert.ToInt32(dr["GoalID"]);

                return info;
            }
            public static BlockInfo GetBlock(DataRow dr)
            {
                BlockInfo info = new BlockInfo();

                info.ID = Convert.ToInt32(dr["BlockID"]);
                info.Title = Convert.ToString(dr["Title"]);
                info.HasDue = Convert.ToBoolean(dr["HasDue"]);
                info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                if (dr["EndDate"] != DBNull.Value) info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                if (dr["DueDate"] != DBNull.Value) info.DueDate = Convert.ToDateTime(dr["DueDate"]);
                if (Convert.ToInt32(dr["ProjectID"]) > 0)
                    info.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                info.ClusterID = Convert.ToInt32(dr["ClusterID"]);
                if (dr["Details"] != DBNull.Value) info.Details = Convert.ToString(dr["Details"]);
                info.Status = (DTC.StatusEnum)Convert.ToInt32(dr["StatusID"]);
                info.Order = Convert.ToInt32(dr["TheOrder"]);
                if (dr["ProjectCode"] != DBNull.Value) info.ProjectCode = Convert.ToString(dr["ProjectCode"]);

                return info;
            }
            public static BlockInfo GetBlock(long blockID)
            {
                BlockInfo info = new BlockInfo();

                string strSQL = "SELECT Blocks.*, Goals.GoalID" +
                    " FROM Blocks " +
                    " LEFT OUTER JOIN Goals ON Blocks.BlockID = Goals.ItemID" +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Blocks.BlockID = " + blockID;
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                    info = GetBlockWithGoal(dt.Rows[0]);

                return info;
            }
            public static int GetBlockXIcon(long blockID)
            {
                int xIConID = 0;

                string strSQL = "SELECT * " +
                     " FROM Blocks " +
                     " WHERE BlockID = " + blockID;
                DataTable dt = RunExecuteReader(strSQL);

                if (dt.Rows.Count == 1)
                    xIConID = Convert.ToInt32(dt.Rows[0]["XIconID"]);

                return xIConID;
            }
            public static List<BlockInfo> GetBlocks(ProjectInfo project, bool sortByBlockName)
            {
                return GetBlocks(project.ID, sortByBlockName);
            }
            public static List<BlockInfo> GetBlocks(int projectID, bool sortByBlockName)
            {
                List<BlockInfo> data = new List<BlockInfo>();

                string strOrderBY = " ORDER BY Blocks.BlockID DESC";
                if (sortByBlockName)
                    strOrderBY = " ORDER BY Blocks.Title ASC";

                string strSQL = "SELECT Blocks.*, Goals.GoalID" +
                    " FROM Blocks " +
                    " LEFT OUTER JOIN Goals ON Blocks.BlockID = Goals.ItemID" +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Block +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Blocks.ProjectID = " + projectID +
                    strOrderBY;

                int order = 1;

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    BlockInfo info = GetBlockWithGoal(dr);

                    info.Order = order;
                    data.Add(info);
                    order++;
                }

                return data;
            }
            public static List<BlockInfo> GetBlocks(int projectID)
            {
                List<BlockInfo> data = new List<BlockInfo>();

                string strSQL = "SELECT * FROM Blocks " +
                    " WHERE ProjectID = " + projectID +
                    " ORDER BY Title ASC";

                int order = 1;

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    BlockInfo info = GetBlock(dr);

                    info.Order = order;
                    data.Add(info);
                    order++;
                }

                return data;
            }
            public static List<BlockInfo> GetBlocksOfCluster(long clusterID)
            {
                List<BlockInfo> data = new List<BlockInfo>();

                string strSQL = "SELECT * FROM Blocks " +
                    " WHERE Blocks.ClusterID = " + clusterID;

                int order = 1;

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    BlockInfo info = GetBlock(dr);

                    info.Order = order;
                    data.Add(info);
                    order++;
                }

                return data;
            }
            public static List<BlockInfo> GetBlocksCompleted(DateTime startDate, DateTime endDate, ProjectInfo project)
            {
                List<BlockInfo> data = new List<BlockInfo>();
                string strProject = string.Empty;
                if (project != null)
                    strProject = " AND Blocks.ProjectID = " + project.ID;

                string strSQL = "SELECT Blocks.*, Goals.GoalID" +
                    " FROM Blocks " +
                    " LEFT OUTER JOIN Goals ON Blocks.BlockID = Goals.ItemID" +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Block +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Blocks.StatusID = " + (int)DTC.StatusEnum.Success +
                    " AND Blocks.EndDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND Blocks.EndDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    strProject +
                    " ORDER BY Blocks.EndDate DESC";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                        data.Add(GetBlockWithGoal(dr));

                return data;
            }
            public static List<BlockInfo> GetBlocksCompleted(long projectID, int numBlocks)
            {
                List<BlockInfo> data = new List<BlockInfo>();

                string strSQL = "SELECT TOP " + numBlocks + " Blocks.*, Goals.GoalID" +
                    " FROM Blocks " +
                    " LEFT OUTER JOIN Goals ON Blocks.BlockID = Goals.ItemID" +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Block +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Blocks.StatusID = " + (int)DTC.StatusEnum.Success +
                    " AND Blocks.ProjectID = " + projectID +
                    " ORDER BY Blocks.EndDate DESC";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetBlockWithGoal(dr));

                return data;
            }
            public static List<BlockInfo> GetBlocksDue(DateTime startDate, int dueDaysForward)
            {
                List<BlockInfo> data = new List<BlockInfo>();
                DateTime dueDate = DateTime.Today.AddDays(dueDaysForward);

                string strSQL = "SELECT Blocks.*, Goals.GoalID" +
                    " FROM Blocks " +
                    " LEFT OUTER JOIN Goals ON Blocks.BlockID = Goals.ItemID" +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Block +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Blocks.HasDue = 1 " +
                    " AND Blocks.StatusID = " + (int)DTC.StatusEnum.Running +
                    " AND Blocks.DueDate <= " + DTC.Date.ObtainGoodDT(dueDate, true) +
                    " ORDER BY Blocks.DueDate ASC";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                        data.Add(GetBlockWithGoal(dr));

                return data;
            }
            public static long AddUpdateBlock(BlockInfo block)
            {
                string SQL = "";

                if (block.ID == 0)
                {
                    int xIconID = Projects.GetProjectXIcon(block.ProjectID);

                    SQL = "SET NOCOUNT ON INSERT INTO Blocks " +
                         " (Title, Details, ProjectID, ClusterID, " +
                         " HasDue, StartDate, EndDate, DueDate, StatusID," +
                         " TheOrder, ProjectCode, XIconID)" +
                         " VALUES (" +
                         "'" + DTC.Control.InputText(block.Title, 255) + "'," +
                         "'" + DTC.Control.InputText(block.Details, 999) + "'," +
                         block.ProjectID + "," +
                         block.ClusterID + "," +
                         Convert.ToInt16(block.HasDue) + "," +
                         DTC.Date.ObtainGoodDT(block.StartDate, true) + "," +
                         DTC.Date.ObtainGoodDT(block.EndDate, true) + "," +
                         DTC.Date.ObtainGoodDT(block.DueDate, true) + "," +
                         (int)block.Status + "," +
                         block.Order + "," +
                         "'" + DTC.Control.InputText(block.ProjectCode, 50) + "'," +
                         xIconID + 
                         ") SELECT SCOPE_IDENTITY() AS BlockID";
                    block.ID = RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Blocks SET " +
                       " Title = '" + DTC.Control.InputText(block.Title, 255) + "'," +
                       " Details = '" + DTC.Control.InputText(block.Details, 999) + "'," +
                       " ProjectID = " + block.ProjectID + "," +
                       " ClusterID = " + block.ClusterID + "," +
                       " HasDue = " + Convert.ToUInt32(block.HasDue) + "," +
                       " StartDate = " + DTC.Date.ObtainGoodDT(block.StartDate, true) + "," +
                       " EndDate = " + DTC.Date.ObtainGoodDT(block.EndDate, true) + "," +
                       " DueDate = " + DTC.Date.ObtainGoodDT(block.DueDate, true) + "," +
                       " StatusID = " + (int)block.Status + "," +
                       " TheOrder = " + block.Order + "," +
                       " ProjectCode = '" + DTC.Control.InputText(block.ProjectCode, 50) + "'" +
                       " WHERE BlockID = " + block.ID;
                    RunNonQuery(SQL);
                }

                return block.ID;
            }
            public static bool CompleteTheBlock(long blockID)
            {
                bool result = true;

                List<SegmentInfo> segments  = GetSegments(blockID);
                if(segments.Exists(q => q.Status == DTC.StatusEnum.Running))
                {
                    result = false;
                }
                else
                {
                    result = true;

                    BlockInfo block = GetBlock(blockID);
                    block.Status = DTC.StatusEnum.Success;
                    block.EndDate = DateTime.Today;
                    
                    AddUpdateBlock(block);
                }

                return result;
            }
            public static bool DeleteBlock(long blockID)
            {
                bool isOK = true;

                string strSQL = "SELECT * FROM Segments " +
                    " WHERE BlockID = " + blockID;
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                    isOK = false;

                if (isOK)
                {
                    strSQL = "DELETE Blocks WHERE BlockID = " + blockID;
                    RunNonQuery(strSQL);
                }

                return isOK;
            }
            public static void ChangeOrder(long blockID, int order)
            {
                string SQL = "UPDATE Blocks SET" +
                    " TheOrder = " + order +
                    " WHERE BlockID = " + blockID;
                RunNonQuery(SQL);
            }
            public static float GetSize(BlockInfo block)
            {
                float totalSize = 0;
                List<SegmentInfo> segments = Segments.GetSegments(block.ID);

                foreach (SegmentInfo segment in segments)
                    totalSize += segment.GetSize();

                return totalSize;
            }
            public static float GetPerformance(BlockInfo block, DateTime cutOffDate)
            {
                // if the segment is closed before cutOffDate, it's great!
                if (block.Status != DTC.StatusEnum.Running && block.EndDate <= cutOffDate)
                {
                    return 100;
                }

                float totalSize = 0;
                float completeness = 0;

                List<SegmentInfo> segments = Segments.GetSegments(block.ID);

                // if we're here, it means that the block is not completed in time
                // now we'll calculate the performance
                foreach (SegmentInfo segment in segments)
                {
                    totalSize += segment.GetSize();

                    // if the segment is completed within time, it adds up completely 
                    // regardless the coumpound tasks in it
                    if (segment.Status != DTC.StatusEnum.Running)
                    {
                        if (segment.HasDue)
                        {
                            if (segment.EndDate <= cutOffDate)
                                completeness += segment.GetSize();
                        }
                        else
                        {
                            completeness += segment.GetSize();
                        }

                    }
                    else
                    {
                        completeness += (Segments.GetPerformance(segment, cutOffDate) * segment.GetSize()) / 100;
                    }
                }

                if (totalSize > 0) return 100 * completeness / totalSize;
                else return 0;
            }
            public static Dictionary<int, BlockPerfInfo> GetAllPerformances(ProjectInfo project)
            {
                Dictionary<int, BlockPerfInfo> dict = new Dictionary<int, BlockPerfInfo>();
                Dictionary<int, SegmentPerfInfo> segmentPerfs = Segments.GetSegmentPerfs(project);

                foreach (SegmentPerfInfo sPerf in segmentPerfs.Values)
                {
                    if (!dict.ContainsKey(sPerf.BlockID))
                        dict.Add(sPerf.BlockID, new BlockPerfInfo());

                    dict[sPerf.BlockID].Size += sPerf.Size;
                    dict[sPerf.BlockID].NumTodos += sPerf.NumTodos;
                    dict[sPerf.BlockID].NumCompleted += sPerf.NumCompleted;
                    dict[sPerf.BlockID].CompletedSize += sPerf.GetCompletedSize();
                }

                return dict;
            }
            public static List<BlockInfo> GetBlocksWithCompletionOrder(ProjectInfo project)
            {
                List<BlockInfo> data = new List<BlockInfo>();
                List<BlockInfo> blocks = GetBlocks(project, false);

                string strSQL = "SELECT BlockID, Title," +
                    " (SELECT ISNULL(SUM(Size), 0) AS Expr1" +
                    " FROM Segments AS S " +
                    " WHERE (B.BlockID = BlockID) AND (StatusID <> " + (int)DTC.StatusEnum.Running + ")) AS COMPLETED_SIZE," +
                    " (SELECT ISNULL(SUM(Size), 0) AS Expr1" +
                    " FROM Segments AS S" +
                    " WHERE (B.BlockID = BlockID)) AS TOTAL_SIZE" +
                    " FROM Blocks AS B" +
                    " WHERE (ProjectID = " + project.ID + ")" +
                    " ORDER BY " +
                    " (SELECT SUM(Size) AS Expr1" +
                    " FROM Segments AS S" +
                    " WHERE (B.BlockID = BlockID) AND (StatusID <> " + (int)DTC.StatusEnum.Running + ")) /" +
                    " (SELECT SUM(Size) AS Expr1" +
                    " FROM Segments AS S" +
                    " WHERE (B.BlockID = BlockID))";

                int order = 1;

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    long blockID = Convert.ToInt32(dr["BlockID"]);
                    BlockInfo info = blocks.Find(i=>i.ID == blockID);
                    if (info != null)
                    {
                        info.Order = order;
                        data.Add(info);
                        order++;
                    }
                    
                }

                return data;
            }
            public static List<BlockInfo> GetBlocksFromStringSplit(string strBlocks)
            {
                List<BlockInfo> data = new List<BlockInfo>();
                List<string> blockIDs = DTC.SplitInto(strBlocks, '|');
                string sqlBlocks = " WHERE BlockID = 0 ";
                
                foreach(string blockID in blockIDs)
                {
                    sqlBlocks += " OR BlockID = " + blockID;
                }
                   
                string strSQL = "SELECT * FROM Blocks " + sqlBlocks;
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(Blocks.GetBlock(dr));

                return data;
            }
        }
         
        public class Segments
        {
            public static long AddUpdateSegment(SegmentInfo segment)
            {
                string strSQL = "";

                if (segment.ID == 0)
                {
                    strSQL = "SET NOCOUNT ON INSERT INTO Segments " +
                        " (Title, Details, BlockID," +
                        " HasDue, StartDate, EndDate, DueDate, StatusID, Size, ChapterID, ProjectCode " +
                        " )" +
                        " VALUES (" +
                        "'" + DTC.Control.InputText(segment.Title, 255) + "'," +
                        "'" + DTC.Control.InputText(segment.Details, 999) + "'," +
                        segment.BlockID + "," +
                        Convert.ToUInt32(segment.HasDue) + "," +
                        DTC.Date.ObtainGoodDT(segment.StartDate, true) + "," +
                        DTC.Date.ObtainGoodDT(segment.EndDate, true) + "," +
                        DTC.Date.ObtainGoodDT(segment.DueDate, true) + "," +
                        (int)segment.Status + "," +
                        (int)segment.Size + "," +
                        segment.ChapterID + "," +
                        "'" + DTC.Control.InputText(segment.ProjectCode, 50) + "'," +
                        ") SELECT SCOPE_IDENTITY() AS SegmentID";
                    segment.ID = RunExecuteScalar(strSQL);
                }
                else
                {
                    strSQL = "UPDATE Segments " +
                         " SET " +
                         " Title = '" + DTC.Control.InputText(segment.Title, 255) + "'," +
                         " Details = '" + DTC.Control.InputText(segment.Details, 999) + "'," +
                         " BlockID = " + segment.BlockID + "," +
                         " HasDue = " + Convert.ToUInt32(segment.HasDue) + "," +
                         " StartDate = " + DTC.Date.ObtainGoodDT(segment.StartDate, true) + "," +
                         " EndDate = " + DTC.Date.ObtainGoodDT(segment.EndDate, true) + "," +
                         " DueDate = " + DTC.Date.ObtainGoodDT(segment.DueDate, true) + "," +
                         " StatusID = " + (int)segment.Status + "," +
                         " Size = " + (int)segment.Size + "," +
                         " ChapterID = " + segment.ChapterID + "," +
                         " ProjectCode = '" + DTC.Control.InputText(segment.ProjectCode, 50) + "' " +
                         " WHERE SegmentID = " + segment.ID;
                    RunNonQuery(strSQL);
                }

                return segment.ID;
            }
            public static SegmentInfo GetSegment(DataRow dr)
            {
                SegmentInfo info = new SegmentInfo();

                info.ID = Convert.ToInt32(dr["SegmentID"]);
                info.Title = Convert.ToString(dr["Title"]);
                info.HasDue = Convert.ToBoolean(dr["HasDue"]);
                info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                if (dr["EndDate"] != DBNull.Value) info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                if (dr["DueDate"] != DBNull.Value) info.DueDate = Convert.ToDateTime(dr["DueDate"]);
                info.BlockID = Convert.ToInt32(dr["BlockID"]);
                if (dr["Details"] != DBNull.Value) info.Details = Convert.ToString(dr["Details"]);
                info.Status = (DTC.StatusEnum)Convert.ToInt32(dr["StatusID"]);
                info.Size = (DTC.SizeEnum)Convert.ToInt32(dr["Size"]);
                if (dr["TheOrder"] != DBNull.Value) info.Order = Convert.ToInt32(dr["TheOrder"]);
                if (dr["ChapterID"] != DBNull.Value) info.ChapterID = Convert.ToInt32(dr["ChapterID"]);
                if (dr["ProjectCode"] != DBNull.Value) info.ProjectCode = Convert.ToString(dr["ProjectCode"]);
                if (dr["GoalID"] != DBNull.Value) info.RunningGoalID = Convert.ToInt32(dr["GoalID"]);

                return info;
            }
            public static List<SegmentInfo> GetSegments(long blockID)
            {
                List<SegmentInfo> data = new List<SegmentInfo>();

                string strSQL = "SELECT Segments.*, Goals.GoalID" +
                    " FROM Segments" +
                    " LEFT OUTER JOIN Goals ON Segments.SegmentID = Goals.ItemID" +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Segments.BlockID = " + blockID +
                    " ORDER BY Segments.TheOrder";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetSegment(dr));
                
                return data;
            }
            public static List<SegmentInfo> GetSegmentsCompleted(DateTime startDate, DateTime endDate, long projectID)
            {
                List<SegmentInfo> data = new List<SegmentInfo>();

                string strProject = string.Empty;
                if (projectID > 0)
                    strProject = " AND Blocks.ProjectID = " + projectID;

                string strSQL = "SELECT Segments.*, Goals.GoalID " +
                    " FROM Segments " +
                    " INNER JOIN Blocks ON Segments.BlockID = Blocks.BlockID " +
                    " LEFT OUTER JOIN Goals ON Segments.SegmentID = Goals.ItemID " +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Segments.StatusID = " + (int)DTC.StatusEnum.Success +
                    " AND Segments.EndDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND Segments.EndDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    strProject +
                    " ORDER BY Segments.EndDate DESC";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    data.Add(GetSegment(dr));
                }

                return data;
            }
            public static List<SegmentInfo> GetSegmentsCompleted(long projectID, int numSegments)
            {
                List<SegmentInfo> data = new List<SegmentInfo>();
                
                string strSQL = "SELECT TOP " +  numSegments + " Segments.*, Goals.GoalID " +
                    " FROM Segments " +
                    " INNER JOIN Blocks ON Segments.BlockID = Blocks.BlockID " +
                    " LEFT OUTER JOIN Goals ON Segments.SegmentID = Goals.ItemID " +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Segments.StatusID = " + (int)DTC.StatusEnum.Success +
                    " AND Blocks.ProjectID = " + projectID +
                    " ORDER BY Segments.EndDate DESC";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetSegment(dr));

                return data;
            }
            public static SegmentInfo GetSegment(long segmentID)
            {
                SegmentInfo info = new SegmentInfo();

                string strSQL = "SELECT Segments.*, Goals.GoalID" +
                    " FROM Segments" +
                    " LEFT OUTER JOIN Goals ON Segments.SegmentID = Goals.ItemID" +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " WHERE Segments.SegmentID = " + segmentID;
                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count > 0)
                    info = GetSegment(dt.Rows[0]);

                return info;
            }
            public static bool DeleteSegment(long segmentID)
            {
                bool isOK = true;

                string strSQL = "SELECT * FROM Tasks WHERE SegmentID = " + segmentID;
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    isOK = false;

                if (isOK)
                {
                    strSQL = "DELETE Segments WHERE SegmentID = " + segmentID;
                    RunNonQuery(strSQL);
                }

                return isOK;
            }
            public static void ChangeOrder(long segmentID, int order)
            {
                string strSQL = "UPDATE Segments SET" +
                    " TheOrder = " + order +
                    " WHERE SegmentID = " + segmentID;
                RunNonQuery(strSQL);
            }
            public static float GetPerformance(SegmentInfo segment, DateTime cutOffDate)
            {
                // if the segment is closed before cutOffDate, it's great!
                if (segment.Status != DTC.StatusEnum.Running && segment.EndDate <= cutOffDate)
                {
                    return 100;
                }

                if (segment.ID == 0) return 0;

                float totalSize = 0;
                float completeness = 0;

                List<TaskInfo> tasks = DB.Tasks.GetTasks("SELECT * FROM Tasks WHERE SegmentID = " + segment.ID);
                float compTodos = 0;
                float inCompTodos = 0;

                totalSize = segment.GetSize();

                // if the segment is not closed in time, let's calculate a performance
                foreach (TaskInfo task in tasks)
                {
                    if (task.IsCompleted)
                    {
                        if (!segment.HasDue)
                            compTodos++;
                        else
                            if (task.EndDate <= cutOffDate)
                            {
                                compTodos++;
                            }
                    }
                    else
                    {
                        inCompTodos++;
                    }
                }

                if (compTodos > 0)
                    completeness = totalSize * compTodos / (compTodos + inCompTodos);

                // in either case, we have the 'completeness' and 'totalSize' figures
                // we can calculate it easily
                if (totalSize > 0) return 100 * completeness / totalSize;
                else return 0;
            }
            public static Dictionary<int, Dictionary<int, SegmentPerfInfo>> GetSegmentPerfs(List<ProjectInfo> projects)
            {
                Dictionary<int, Dictionary<int, SegmentPerfInfo>> segmentPerfs = new Dictionary<int, Dictionary<int, SegmentPerfInfo>>();

                string filterSQL = "";
                if (projects.Count < 10)
                {
                    filterSQL = " AND (Projects.ProjectID = -1 " + filterSQL;

                    foreach (ProjectInfo p in projects)
                        filterSQL += " OR Projects.ProjectID = " + p.ID;

                    filterSQL = filterSQL + ")";
                }

                string strSQL = "SELECT Projects.ProjectID,  " +
                    " Segments.SegmentID, Segments.BlockID, Segments.Size, Tasks.IsCompleted AS TASK_STATUS, " +
                    " Segments.StatusID AS SEGMENT_STATUS, COUNT(Tasks.TaskID) AS TOTAL" +
                    " FROM Projects" +
                    " INNER JOIN Blocks ON Projects.ProjectID = Blocks.ProjectID" +
                    " INNER JOIN Segments ON Blocks.BlockID = Segments.BlockID" +
                    " LEFT OUTER JOIN Tasks ON Segments.SegmentID = Tasks.SegmentID" +
                    " WHERE Projects.IsActive = 1" +
                    filterSQL +
                    " GROUP BY Tasks.IsCompleted, Projects.ProjectID, " +
                    " Segments.SegmentID, Segments.BlockID, Segments.Size, " +
                    " Segments.StatusID, Tasks.TaskID";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    int projectID = Convert.ToInt32(dr["ProjectID"]);
                    int segmentID = Convert.ToInt32(dr["SegmentID"]);

                    if (!segmentPerfs.ContainsKey(projectID))
                        segmentPerfs.Add(projectID, new Dictionary<int, SegmentPerfInfo>());

                    SegmentPerfInfo segmentPerf = new SegmentPerfInfo();
                    if (!segmentPerfs[projectID].ContainsKey(segmentID))
                    {
                        segmentPerf.SegmentID = segmentID;
                        segmentPerf.BlockID = Convert.ToInt32(dr["BlockID"]);
                        segmentPerf.Status = (DTC.StatusEnum)Convert.ToInt16(dr["SEGMENT_STATUS"]);
                        segmentPerf.Size = Convert.ToSingle(dr["Size"]);
                        segmentPerfs[projectID].Add(segmentID, segmentPerf);
                    }
                    else
                    {
                        segmentPerf = segmentPerfs[projectID][segmentID];
                    }

                    segmentPerf.NumTodos += Convert.ToSingle(dr["TOTAL"]);
                    if (dr["TASK_STATUS"] != DBNull.Value)
                    {
                        if (Convert.ToBoolean(dr["TASK_STATUS"]))
                            segmentPerf.NumCompleted += Convert.ToSingle(dr["TOTAL"]);
                    }

                }
                
                return segmentPerfs;
            }
            public static Dictionary<int, SegmentPerfInfo> GetSegmentPerfs(ProjectInfo project)
            {
                Dictionary<int, SegmentPerfInfo> segmentPerfs = new Dictionary<int, SegmentPerfInfo>();

                List<ProjectInfo> projects = new List<ProjectInfo>();
                projects.Add(project);

                Dictionary<int, Dictionary<int, SegmentPerfInfo>> allSegmentPerfs = GetSegmentPerfs(projects);
                if (allSegmentPerfs.ContainsKey(project.ID))
                    segmentPerfs = allSegmentPerfs[project.ID];

                return segmentPerfs;
            }
            public static List<SegmentInfo> GetSegments(int projectID)
            {
                List<SegmentInfo> data = new List<SegmentInfo>();

                string strSQL = "SELECT Segments.*, Goals.GoalID" +
                    " FROM Segments" +
                    " INNER JOIN Blocks ON Segments.BlockID = Blocks.BlockID" +
                    " LEFT OUTER JOIN Goals ON Segments.SegmentID = Goals.ItemID" +
                    " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " WHERE Blocks.ProjectID = " + projectID +
                    " ORDER BY Blocks.TheOrder, Segments.TheOrder";
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetSegment(dr));
                
                return data;
            }
        }

        public class Days
        {
            /// <summary>
            /// Gets a dictionary of days within the date interval
            /// </summary>
            /// <param name="startDate">Start Date of the query GEQ</param>
            /// <param name="endDate">End Date of the query LEQ</param>
            /// <returns></returns>
            public static List<DayInfo> GetDays(DateTime startDate, DateTime endDate, bool create)
            {
                List<DayInfo> list = new List<DayInfo>();

                string SQL = "SELECT * FROM Days " +
                    " WHERE TheDay >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TheDay <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ORDER BY TheDay";
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    DayInfo info = GetDay(dr);
                    list.Add(info);
                }
                

                DateTime date = startDate;
                while (date <= endDate)
                {
                    if (list.Find(delegate (DayInfo day) { return day.TheDate.Date == date.Date; }) == null)
                    {
                        DayInfo newDay = new DayInfo(date);
                        if (create)
                        {
                            newDay.DayID = AddUpdateDay(newDay);
                        }
                        list.Add(newDay);
                    }
                    date = date.AddDays(1);
                }

                list = UpdateSleepAmounts(list);
                list = Tasks.UpdateDailyTotals(list, startDate, endDate);
                list = Tasks.UpdateThings(list, startDate, endDate);
                list = Tasks.UpdateIdeas(list, startDate, endDate);
                list = Tasks.UpdateTasksCompletedPerDay(list, startDate, endDate);

                return list;
            }
            /// <summary>
            /// Gets dictionary of days in a month with full data.
            /// </summary>
            public static List<DayInfo> GetDaysOfMonth(MonthInfo month)
            {
                return GetDays(month.StartDate, month.EndDate, false);
            }
            /// <summary>
            /// Gets a day with full data.
            /// </summary>
            public static DayInfo GetDay(DateTime theDate, bool create)
            {
                DayInfo info = new DayInfo(theDate);
                List<DayInfo> days = new List<DayInfo>();

                days = GetDays(theDate, theDate, create);
                foreach (DayInfo day in days)
                {
                    info = day;
                }

                return info;
            }
            public static DayInfo GetDay(int dayID)
            {
                DayInfo info = new DayInfo();
                string SQL = "SELECT * FROM Days WHERE DayID=" + dayID;
                DataTable dt = RunExecuteReader(SQL);
                foreach (DataRow dr in dt.Rows)
                {
                    info = GetDay(dr);
                }
                

                return info;
            }
            /// <summary>
            /// Returns a DayInfo instance with a given DataRow
            /// </summary>
            /// <param name="dr">DataRow</param>
            /// <returns>DayInfo</returns>
            public static DayInfo GetDay(DataRow dr)
            {
                DayInfo info = new DayInfo();

                info.DayID = Convert.ToInt32(dr["DayID"]);
                info.TheDate = Convert.ToDateTime(dr["TheDay"]);
                if (dr["StartInstance"] != DBNull.Value) info.StartInstance = Convert.ToDateTime(dr["StartInstance"]);
                if (dr["EndInstance"] != DBNull.Value) info.EndInstance = Convert.ToDateTime(dr["EndInstance"]);
                info.Weight = Convert.ToSingle(dr["TheWeight"]);
                info.SleepAmount = Convert.ToSingle(dr["SleepAmount"]);
                info.Theme = Convert.ToString(dr["Theme"]);
                info.Label = Convert.ToString(dr["Label"]);
                info.Diary = Convert.ToString(dr["Diary"]);
                info.Performance = Convert.ToInt16(dr["Performance"]);
                info.PerfWeek = Convert.ToInt16(dr["PerfWeek"]);
                info.PerfMonth = Convert.ToInt16(dr["PerfMonth"]);
                info.PerfQuarter = Convert.ToInt16(dr["PerfQuarter"]);
                info.PerfYear = Convert.ToInt16(dr["PerfYear"]);
                info.PerfDecade = Convert.ToInt16(dr["PerfDecade"]);
                info.PerfLife = Convert.ToInt16(dr["PerfLife"]);
                return info;
            }
            /// <summary>
            /// Calculates and updates the SleepAmount values
            /// </summary>
            /// <param name="days">Days dictionary</param>
            /// <returns>Days dictionary</returns>
            public static List<DayInfo> UpdateSleepAmounts(List<DayInfo> days)
            {
                #region Finding the "first end instance"
                DateTime minimumDate = DateTime.MaxValue;

                for (int i = 0; i < days.Count; i++)
                {
                    if (days[i].TheDate < minimumDate)
                        minimumDate = days[i].TheDate;
                }

                DateTime initialEndInstance = DateTime.MinValue;
                DateTime initialDate = DateTime.MinValue;

                List<DayInfo> list = new List<DayInfo>();

                string SQL = "SELECT * FROM Days " +
                    " WHERE TheDay = " + DTC.Date.ObtainGoodDT(minimumDate.AddDays(-1), true);
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["EndInstance"] != DBNull.Value)
                    {
                        initialDate = Convert.ToDateTime(dr["TheDay"]);
                        initialEndInstance = Convert.ToDateTime(dr["EndInstance"]);
                    }
                }
                #endregion

                #region Calculating sleep amounts
                foreach (DayInfo day in days)
                {
                    // is there a previous instance
                    DateTime previousEndInstance = DateTime.MinValue;
                    foreach (DayInfo cDays in days)
                    {
                        if (cDays.TheDate == day.TheDate.AddDays(-1))
                            previousEndInstance = cDays.EndInstance;
                    }
                    // may be the first day, so we should check the very first end instance
                    // that is calculated above
                    if (initialDate == day.TheDate.AddDays(-1))
                        previousEndInstance = initialEndInstance;

                    DateTime thisStartInstance = day.StartInstance;

                    if (previousEndInstance > DateTime.MinValue
                        && thisStartInstance > DateTime.MinValue)
                    {
                        TimeSpan ts = thisStartInstance.Subtract(previousEndInstance);
                        day.SleepAmount = (float)ts.TotalHours;
                        if (day.SleepAmount < 0) day.SleepAmount = 0;
                        if (day.SleepAmount > 16) day.SleepAmount = 0;
                        if (day.StartInstance == day.EndInstance) day.SleepAmount = 0;
                    }
                }
                #endregion

                return days;
            }
            /// <summary>
            /// Adds/Updates a new day.
            /// The decision whether add or update will be applied, is made here.
            /// </summary>
            public static int AddUpdateDay(DayInfo day)
            {
                AddOrUpdate addOrUpdate = AddOrUpdate.Add;
                int dayID = 0;

                string SQL = "SELECT * FROM Days " +
                    " WHERE TheDay = " + DTC.Date.ObtainGoodDT(day.TheDate, true);
                DataTable dt = RunExecuteReader(SQL);
                foreach (DataRow dr in dt.Rows)
                {
                    addOrUpdate = AddOrUpdate.Update;
                    dayID = Convert.ToInt32(dr["DayID"]);
                }

                if (day.StartInstance <= DateTime.MinValue) day.StartInstance = day.TheDate;
                if (day.EndInstance <= DateTime.MinValue) day.EndInstance = day.TheDate;

                if (addOrUpdate == AddOrUpdate.Add)
                {
                    SQL = "INSERT INTO Days " +
                        " (TheDay,StartInstance,EndInstance,TheWeight,SleepAmount," +
                        " Label,Theme,Diary,Performance, " +
                        " PerfWeek, PerfMonth, PerfQuarter, PerfYear, PerfDecade, PerfLife" +
                        " ) VALUES (" +
                        DTC.Date.ObtainGoodDT(day.TheDate, true) + "," +
                        DTC.Date.ObtainGoodDT(day.StartInstance, false) + "," +
                        DTC.Date.ObtainGoodDT(day.EndInstance, false) + "," +
                        DTC.Control.CommaToDot(day.Weight.ToString()) + "," +
                        DTC.Control.CommaToDot(day.SleepAmount.ToString()) + "," +
                        "'" + DTC.Control.InputText(day.Label, 255) + "'," +
                        "'" + DTC.Control.InputText(day.Theme, 255) + "'," +
                        "'" + DTC.Control.InputTextLight(day.Diary, 500) + "'," +
                        day.Performance + "," +
                        day.PerfWeek + "," +
                        day.PerfMonth + "," +
                        day.PerfQuarter + "," +
                        day.PerfYear + "," +
                        day.PerfDecade + "," +
                        day.PerfLife +
                        ")  SELECT SCOPE_IDENTITY() AS DayID";
                    dayID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Days SET " +
                        " TheDay = " + DTC.Date.ObtainGoodDT(day.TheDate, true) + "," +
                        " StartInstance = " + DTC.Date.ObtainGoodDT(day.StartInstance, false) + "," +
                        " EndInstance = " + DTC.Date.ObtainGoodDT(day.EndInstance, false) + "," +
                        " TheWeight = " + DTC.Control.CommaToDot(day.Weight.ToString()) + "," +
                        " SleepAmount = " + DTC.Control.CommaToDot(day.SleepAmount.ToString()) + "," +
                        " Theme = '" + DTC.Control.InputText(day.Theme, 255) + "'," +
                        " Label = '" + DTC.Control.InputText(day.Label, 255) + "'," +
                        " Diary = '" + DTC.Control.InputTextLight(day.Diary, 500) + "'," +
                        " Performance = " + day.Performance + "," +
                        " PerfWeek = " + day.PerfWeek + "," +
                        " PerfMonth = " + day.PerfMonth + "," +
                        " PerfQuarter = " + day.PerfQuarter + "," +
                        " PerfYear = " + day.PerfYear + "," +
                        " PerfDecade = " + day.PerfDecade + "," +
                        " PerfLife = " + day.PerfLife +
                        " WHERE DayID = " + dayID;
                    RunNonQuery(SQL);
                }

                return dayID;
            }
            public static float CalculateWakeUpEx(TimeInfo thresholdTime, DateTime startDate, DateTime endDate)
            {
                float result = 0;

                string SQL = "SELECT StartInstance FROM Days " +
                    " WHERE TheDay >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TheDay <= " + DTC.Date.ObtainGoodDT(endDate, true);
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    DateTime theTime = Convert.ToDateTime(dr["StartInstance"]);
                    float startInstance = new TimeInfo(theTime.Hour, theTime.Minute).GetDecimalValue();
                    if (startInstance > 0)
                    {
                        float deltaT = (startInstance - thresholdTime.GetDecimalValue());
                        float deltaX = DTC.DTMath.GradualDecline(deltaT);
                        result += deltaX;
                    }
                }
                
                return result;
            }
        }

        public class Weeks
        {
            public static List<WeekInfo> GetWeeks(DateTime startDate, DateTime endDate, bool create)
            {
                List<WeekInfo> list = new List<WeekInfo>();

                // date parameters may be out of sync,
                // thus we fix it to the week start points
                startDate = new WeekInfo(startDate).StartDate.Date;
                endDate = new WeekInfo(endDate).StartDate.Date;

                string SQL = "SELECT * FROM Weeks " +
                    " WHERE StartDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND StartDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ORDER BY StartDate";
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    WeekInfo info = GetWeek(dr);
                    list.Add(info);
                }

                DateTime date = new WeekInfo(startDate).StartDate;

                while (date <= endDate)
                {
                    if (list.Find(delegate (WeekInfo week) { return week.StartDate.Date == date.Date; }) == null)
                    {
                        WeekInfo newWeek = new WeekInfo(date);
                        if (create)
                        {
                            newWeek.WeekID = AddUpdateWeek(newWeek);
                        }
                        list.Add(newWeek);
                    }
                    date = date.AddDays(7);
                }

                return list;
            }
            /// <summary>
            /// Gets a week with full data.
            /// </summary>
            public static WeekInfo GetWeek(DateTime theDate, bool create)
            {
                WeekInfo info = new WeekInfo(theDate);
                theDate = info.StartDate;

                string SQL = "SELECT * FROM Weeks " +
                    " WHERE StartDate = " + DTC.Date.ObtainGoodDT(theDate, true);
                DataTable dt = RunExecuteReader(SQL);
                if(dt.Rows.Count == 1)
                {
                    info = GetWeek(dt.Rows[0]);
                }
                else
                {
                    info = new WeekInfo(theDate);
                    if (create)
                    {
                        info.WeekID = AddUpdateWeek(info);
                    }
                }
                

                return info;
            }
            public static WeekInfo GetWeek(int weekID)
            {
                WeekInfo info = new WeekInfo();
                string SQL = "SELECT * FROM Weeks WHERE WeekID = " + weekID;

                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    info = GetWeek(dr);
                }
                

                return info;
            }
            /// <summary>
            /// Returns a WeekInfo instance with a given DataRow
            /// </summary>
            /// <param name="dr">DataRow</param>
            /// <returns>WeekInfo</returns>
            public static WeekInfo GetWeek(DataRow dr)
            {
                WeekInfo info = new WeekInfo(Convert.ToDateTime(dr["StartDate"]));
                info.WeekID = Convert.ToInt32(dr["WeekID"]);
                info.Label = Convert.ToString(dr["Label"]);
                info.Theme = Convert.ToString(dr["Theme"]);
                info.Performance = Convert.ToInt16(dr["Performance"]);

                return info;
            }
            /// <summary>
            /// Adds/Updates a week.
            /// The decision whether add or update will be applied, is made here.
            /// </summary>
            public static int AddUpdateWeek(WeekInfo week)
            {
                AddOrUpdate addOrUpdate = AddOrUpdate.Add;
                int weekID = 0;

                string SQL = "SELECT * FROM Weeks " +
                    " WHERE StartDate = " + DTC.Date.ObtainGoodDT(week.StartDate, true);
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    addOrUpdate = AddOrUpdate.Update;
                    weekID = Convert.ToInt32(dr["WeekID"]);
                }
                
                if (addOrUpdate == AddOrUpdate.Add)
                {
                    SQL = "SET NOCOUNT ON INSERT INTO Weeks " +
                        " (StartDate,Label,Theme,Performance" +
                        " ) VALUES (" +
                        DTC.Date.ObtainGoodDT(week.StartDate, true) + "," +
                        "'" + DTC.Control.InputText(week.Label, 255) + "'," +
                        "'" + DTC.Control.InputText(week.Theme, 255) + "'," +
                        week.Performance +
                        ")  SELECT SCOPE_IDENTITY() AS WeekID";
                    weekID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Weeks SET " +
                        " StartDate = " + DTC.Date.ObtainGoodDT(week.StartDate, true) + "," +
                        " Theme = '" + DTC.Control.InputText(week.Theme, 255) + "'," +
                        " Label = '" + DTC.Control.InputText(week.Label, 255) + "'," +
                        " Performance = " + week.Performance +
                        " WHERE WeekID = " + weekID;
                    RunNonQuery(SQL);
                }

                return weekID;
            }
            public static float GetWeeklyWeightAverage(WeekInfo week)
            {
                float weight = 0;

                string SQL = "SELECT AVG(TheWeight) AS WEIGHT" +
                    " FROM Days" +
                    " WHERE TheWeight > 0 " +
                    " AND TheDay >= " + DTC.Date.ObtainGoodDT(week.StartDate, true) +
                    " AND TheDay <= " + DTC.Date.ObtainGoodDT(week.EndDate, true);
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[0] != DBNull.Value)
                    {
                        weight = Convert.ToSingle(dr[0]);
                    }
                }
                

                return weight;
            }
            public static float GetWeeklySleepAverage(WeekInfo week)
            {
                float sleep = 0;

                string SQL = "SELECT AVG(SleepAmount) AS SLEEP" +
                    " FROM Days" +
                    " WHERE SleepAmount > 0 " +
                    " AND TheDay >= " + DTC.Date.ObtainGoodDT(week.StartDate, true) +
                    " AND TheDay <= " + DTC.Date.ObtainGoodDT(week.EndDate, true);
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[0] != DBNull.Value)
                    {
                        sleep = Convert.ToSingle(dr[0]);
                    }
                }
                

                return sleep;
            }
        }

        public class Months
        {
            public static List<MonthInfo> GetMonths(DateTime startDate, DateTime endDate, bool create)
            {
                List<MonthInfo> list = new List<MonthInfo>();

                // date parameters may be out of sync,
                // thus we fix it to the week start points
                startDate = new MonthInfo(startDate).StartDate.Date;
                endDate = new MonthInfo(endDate).StartDate.Date;

                string SQL = "SELECT * FROM Months " +
                    " WHERE StartDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND StartDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ORDER BY StartDate";
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    MonthInfo info = GetMonth(dr);
                    list.Add(info);
                }
                

                DateTime date = new MonthInfo(startDate).StartDate;

                while (date <= endDate)
                {
                    if (list.Find(delegate (MonthInfo month) { return month.StartDate.Date == date.Date; }) == null)
                    {
                        MonthInfo newMonth = new MonthInfo(date);
                        if (create)
                        {
                            newMonth.MonthID = AddUpdateMonth(newMonth);
                        }
                        list.Add(newMonth);
                    }
                    date = date.AddMonths(1);
                }

                return list;
            }
            /// <summary>
            /// Gets a month with full data.
            /// </summary>
            public static MonthInfo GetMonth(DateTime theDate, bool create)
            {
                MonthInfo info = new MonthInfo(theDate);
                List<MonthInfo> months = new List<MonthInfo>();

                months = GetMonths(theDate, theDate, create);

                foreach (MonthInfo month in months)
                {
                    info = month;
                }

                return info;
            }
            public static MonthInfo GetMonth(int monthID)
            {
                MonthInfo info = new MonthInfo();
                string SQL = "SELECT * FROM Months WHERE MonthID = " + monthID;

                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    info = GetMonth(dr);
                }
                

                return info;
            }
            /// <summary>
            /// Returns a MonthInfo instance with a given DataRow
            /// </summary>
            /// <param name="dr">DataRow</param>
            /// <returns>MonthInfo</returns>
            public static MonthInfo GetMonth(DataRow dr)
            {
                MonthInfo info = new MonthInfo(Convert.ToDateTime(dr["StartDate"]));

                info.MonthID = Convert.ToInt32(dr["MonthID"]);
                info.Label = Convert.ToString(dr["Label"]);
                info.Theme = Convert.ToString(dr["Theme"]);
                info.Performance = Convert.ToInt32(dr["Performance"]);

                info.Days.Clear();
                for (int i = 1; i <= info.HowManyDays; i++)
                    info.Days.Add(new DayInfo(info.StartDate.AddDays(i)));

                return info;
            }
            /// <summary>
            /// Adds/Updates a month.
            /// The decision whether add or update will be applied, is made here.
            /// </summary>
            public static int AddUpdateMonth(MonthInfo month)
            {
                AddOrUpdate addOrUpdate = AddOrUpdate.Add;
                int monthID = 0;

                string SQL = "SELECT * FROM Months " +
                    " WHERE StartDate = " + DTC.Date.ObtainGoodDT(month.StartDate, true);
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    addOrUpdate = AddOrUpdate.Update;
                    monthID = Convert.ToInt32(dr["MonthID"]);
                }
                

                if (addOrUpdate == AddOrUpdate.Add)
                {
                    SQL = "INSERT INTO Months " +
                        " (StartDate,Label,Theme, Performance)" +
                        " VALUES (" +
                        DTC.Date.ObtainGoodDT(month.StartDate, true) + "," +
                        "'" + DTC.Control.InputText(month.Label, 255) + "'," +
                        "'" + DTC.Control.InputText(month.Theme, 255) + "'," +
                        month.Performance +
                        ") SELECT SCOPE_IDENTITY() AS MonthID";
                    monthID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Months SET " +
                        " Theme = '" + DTC.Control.InputText(month.Theme, 255) + "'," +
                        " Label = '" + DTC.Control.InputText(month.Label, 255) + "'," +
                        " Performance = " + month.Performance +
                        " WHERE MonthID = " + monthID;
                    RunNonQuery(SQL);
                }

                return monthID;
            }
            /// <summary>
            /// Updates WEIGHT and SLEEP averages
            /// </summary>
            /// <param name="month">The month</param>
            /// <returns>Months</returns>
            public static MonthInfo UpdateAverages(MonthInfo month)
            {
                List<MonthInfo> months = new List<MonthInfo>();
                months.Add(month);
                months = UpdateAverages(months);
                return months[0];
            }
            public static List<MonthInfo> UpdateAverages(List<MonthInfo> months)
            {
                DateTime startDate = DateTime.MaxValue, endDate = DateTime.MinValue;
                string SQL = "";

                //find the xtremes
                foreach (MonthInfo month in months)
                {
                    if (month.StartDate < startDate) startDate = month.StartDate;
                    if (month.EndDate > endDate) endDate = month.EndDate;
                }

                #region WEIGHT
                SQL = "SELECT AVG(TheWeight) AS AVGWEIGHT, MIN(TheWeight) AS MINWEIGHT, MAX(TheWeight) AS MAXWEIGHT, " +
                          " YEAR(TheDay) AS THEYEAR, MONTH(TheDay) AS THEMONTH" +
                          " FROM Days" +
                          " WHERE (TheWeight IN" +
                          "   (SELECT     TOP (100) PERCENT TheWeight" +
                          "    FROM Days AS days_1" +
                          "    WHERE (TheWeight > 0)" +
                          "    ORDER BY TheDay DESC)) " +
                          "    AND TheDay >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                          "    AND TheDay <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                          " GROUP BY YEAR(TheDay), MONTH(TheDay)" +
                          " ORDER BY THEYEAR, THEMONTH";
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    foreach (MonthInfo month in months)
                    {
                        int m = 0, y = 0;
                        if (dr["THEYEAR"] != DBNull.Value) y = Convert.ToUInt16(dr["THEYEAR"]);
                        if (dr["THEMONTH"] != DBNull.Value) m = Convert.ToUInt16(dr["THEMONTH"]);

                        if (y == month.Year && m == month.Month)
                        {
                            month.AvgWeight = Convert.ToSingle(dr["AVGWEIGHT"]);
                            month.MinWeight = Convert.ToSingle(dr["MINWEIGHT"]);
                            month.MaxWeight = Convert.ToSingle(dr["MAXWEIGHT"]);
                        }
                    }
                }

                #endregion

                #region SLEEP
                SQL = "SELECT AVG(SleepAmount) AS AVGSLEEP," +
                          " YEAR(TheDay) AS THEYEAR, MONTH(TheDay) AS THEMONTH" +
                          " FROM Days" +
                          " WHERE (SleepAmount IN" +
                          "   (SELECT TOP (100) PERCENT SleepAmount" +
                          "    FROM Days AS days_1" +
                          "    WHERE (SleepAmount > 0)" +
                          "    ORDER BY TheDay DESC)) " +
                          "    AND TheDay >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                          "    AND TheDay <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                          " GROUP BY YEAR(TheDay), MONTH(TheDay)" +
                          " ORDER BY THEYEAR, THEMONTH";
                dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    foreach (MonthInfo month in months)
                    {
                        int m = 0, y = 0;
                        if (dr["THEYEAR"] != DBNull.Value) y = Convert.ToUInt16(dr["THEYEAR"]);
                        if (dr["THEMONTH"] != DBNull.Value) m = Convert.ToUInt16(dr["THEMONTH"]);

                        if (y == month.Year && m == month.Month)
                        {
                            month.AvgSleep = Convert.ToSingle(dr["AVGSLEEP"]);
                        }
                    }
                }

                #endregion
                 

                return months;
            }
        }

        public class Quarters
        {
            public static List<QuarterInfo> GetQuarters(DateTime startDate, DateTime endDate, bool create)
            {
                List<QuarterInfo> list = new List<QuarterInfo>();

                // date parameters may be out of sync,
                // thus we fix it to the week start points
                startDate = new QuarterInfo(startDate).StartDate.Date;
                endDate = new QuarterInfo(endDate).StartDate.Date;

                string SQL = "SELECT * FROM Quarters " +
                    " WHERE StartDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND StartDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ORDER BY StartDate";
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    QuarterInfo info = GetQuarter(dr);
                    list.Add(info);
                }
   

                DateTime date = new QuarterInfo(startDate).StartDate;

                while (date <= endDate)
                {
                    if (list.Find(delegate (QuarterInfo quarter) { return quarter.StartDate.Date == date.Date; }) == null)
                    {
                        QuarterInfo newQuarter = new QuarterInfo(date);
                        if (create)
                        {
                            newQuarter.QuarterID = AddUpdateQuarter(newQuarter);
                        }
                        list.Add(newQuarter);
                    }
                    date = DTC.Date.GetNextQuarter(new QuarterInfo(date)).StartDate;
                }

                return list;
            }
            public static QuarterInfo GetQuarter(DateTime theDate, bool create)
            {
                QuarterInfo info = new QuarterInfo(theDate);
                string SQL = "SELECT * FROM Quarters WHERE StartDate = "
                    + DTC.Date.ObtainGoodDT(info.StartDate.Date, true);
                DataTable dt = RunExecuteReader(SQL);
                if(dt.Rows.Count > 0)
                {
                    info = GetQuarter(dt.Rows[0]);
                }
                else
                {
                    if (create)
                    {
                        info.QuarterID = AddUpdateQuarter(info);
                    }
                }

                return info;
            }
            public static QuarterInfo GetQuarter(int quarterID)
            {
                QuarterInfo info = new QuarterInfo();
                string SQL = "SELECT * FROM Quarters WHERE QuarterID = " + quarterID;
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                    info = GetQuarter(dr);


                return info;
            }
            public static QuarterInfo GetQuarter(DataRow dr)
            {
                QuarterInfo info = new QuarterInfo(Convert.ToDateTime(dr["StartDate"]));

                info.QuarterID = Convert.ToInt32(dr["QuarterID"]);
                info.Theme = Convert.ToString(dr["Theme"]);
                info.Label = Convert.ToString(dr["Label"]);
                info.Performance = Convert.ToInt16(dr["Performance"]);

                return info;
            }
            public static int AddUpdateQuarter(QuarterInfo quarter)
            {
                AddOrUpdate addOrUpdate = AddOrUpdate.Add;
                int quarterID = 0;

                string SQL = "SELECT * FROM Quarters WHERE StartDate = "
                    + DTC.Date.ObtainGoodDT(quarter.StartDate.Date, true);
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    addOrUpdate = AddOrUpdate.Update;
                    quarterID = Convert.ToInt32(dr["QuarterID"]);
                }

                if (addOrUpdate == AddOrUpdate.Add)
                {
                    SQL = "SET NOCOUNT ON INSERT INTO Quarters " +
                        " (StartDate, Label, Theme, Performance" +
                        " ) VALUES (" +
                        DTC.Date.ObtainGoodDT(quarter.StartDate, true) + "," +
                        "'" + DTC.Control.InputText(quarter.Label, 255) + "'," +
                        "'" + DTC.Control.InputText(quarter.Theme, 255) + "'," +
                        quarter.Performance +
                        ")  SELECT SCOPE_IDENTITY() AS YearID";
                    quarterID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Quarters SET " +
                        " StartDate = " + DTC.Date.ObtainGoodDT(quarter.StartDate, true) + "," +
                        " Theme = '" + DTC.Control.InputText(quarter.Theme, 255) + "'," +
                        " Label = '" + DTC.Control.InputText(quarter.Label, 255) + "'," +
                        " Performance = " + quarter.Performance +
                        " WHERE QuarterID = " + quarterID;
                    RunNonQuery(SQL);
                }

                return quarterID;
            }
        }

        public class Years
        {
            public static YearInfo GetYear(DateTime theDate, bool create)
            {
                YearInfo info = new YearInfo(theDate.Year);
                string SQL = "SELECT * FROM Years WHERE YearNO = " + info.Year;
                DataTable dt = RunExecuteReader(SQL);
                if (dt.Rows.Count > 0)
                {
                    info = GetYear(dt.Rows[0]);
                }
                else
                {
                    if (create)
                    {
                        info.YearID = AddUpdateYear(info);
                    }
                }

                return info;
            }
            public static YearInfo GetYear(int yearID)
            {
                YearInfo info = new YearInfo();
                string SQL = "SELECT * FROM Years WHERE YearID = " + yearID;
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    info = GetYear(dr);
                }

                return info;
            }
            public static YearInfo GetYear(DataRow dr)
            {
                YearInfo info = new YearInfo(Convert.ToInt32(dr["YearNO"]));

                info.YearID = Convert.ToInt32(dr["YearID"]);
                info.Label = Convert.ToString(dr["Label"]);
                info.Theme = Convert.ToString(dr["Theme"]);
                info.Performance = Convert.ToInt16(dr["Performance"]);

                return info;
            }
            public static int AddUpdateYear(YearInfo year)
            {
                AddOrUpdate addOrUpdate = AddOrUpdate.Add;
                int yearID = 0;

                string SQL = "SELECT * FROM Years WHERE YearNO = " + year.Year;
                DataTable dt = RunExecuteReader(SQL);
                foreach (DataRow dr in dt.Rows)
                {
                    addOrUpdate = AddOrUpdate.Update;
                    yearID = Convert.ToInt32(dr["YearID"]);
                }
                
                if (addOrUpdate == AddOrUpdate.Add)
                {
                    SQL = "SET NOCOUNT ON INSERT INTO Years " +
                        " (YearNO, Label, Theme, Performance" +
                        " ) VALUES (" +
                        year.Year + "," +
                        "'" + DTC.Control.InputText(year.Label, 255) + "'," +
                        "'" + DTC.Control.InputText(year.Theme, 255) + "'," +
                        year.Performance +
                        ")  SELECT SCOPE_IDENTITY() AS YearID";
                    yearID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Years SET " +
                        " YearNO = " + year.Year + "," +
                        " Theme = '" + DTC.Control.InputText(year.Theme, 255) + "'," +
                        " Label = '" + DTC.Control.InputText(year.Label, 255) + "'," +
                        " Performance = " + year.Performance +
                        " WHERE YearID = " + yearID;
                    RunNonQuery(SQL);
                }

                return yearID;
            }
            public static void GetYearlyFigures(YearInfo year, out float avgWeight, out float minWeight, out float maxWeight, out float avgSleep)
            {
                avgSleep = 0;
                avgWeight = 0;
                minWeight = 0;
                maxWeight = 0;

                string strSQL = "SELECT AVG(SleepAmount) AS AVG_SLEEP, AVG(TheWeight) AS AVG_WEIGHT, " +
                    " MIN(TheWeight) AS MIN_WEIGHT, MAX(TheWeight) AS MAX_WEIGHT," +
                    " MONTH(Days.TheDay) AS M " +
                    " FROM Days " +
                    " WHERE SleepAmount > 0  AND TheWeight > 0  AND YEAR(TheDay) = " + year.Year +
                    " GROUP BY MONTH(TheDay)";

                int n = 0;

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AVG_WEIGHT"] != DBNull.Value) avgWeight += Convert.ToSingle(dr["AVG_WEIGHT"]);
                    if (dr["MIN_WEIGHT"] != DBNull.Value) minWeight += Convert.ToSingle(dr["MIN_WEIGHT"]);
                    if (dr["MAX_WEIGHT"] != DBNull.Value) maxWeight += Convert.ToSingle(dr["MAX_WEIGHT"]);
                    if (dr["AVG_SLEEP"] != DBNull.Value) avgSleep += Convert.ToSingle(dr["AVG_SLEEP"]);
                    n++;
                }
                avgWeight = avgWeight / n;
                minWeight = minWeight / n;
                maxWeight = maxWeight / n;
                avgSleep = avgSleep / n;
            }
        }

        public class Goals
        {
            public static GoalGroupInfo GetGoalGroup(DataRow dr)
            {
                GoalGroupInfo goalGroup = new GoalGroupInfo();

                goalGroup.ID = Convert.ToInt32(dr["GoalGroupID"]);
                goalGroup.Name = Convert.ToString(dr["GoalGroupName"]);
                goalGroup.Code = Convert.ToString(dr["GoalGroupCode"]);
                goalGroup.Order = Convert.ToInt32(dr["GoalGroupOrder"]);
                goalGroup.Leverage = (DTC.SizeEnum)Convert.ToInt16(dr["Leverage"]);

                return goalGroup;
            }
            public static void UpdateGoalGroup(GoalGroupInfo gg)
            {
                string SQL = "UPDATE GoalGroups SET " +
                    " GoalGroupName = '" + DTC.Control.InputText(gg.Name, 50) + "'," +
                    " GoalGroupCode = '" + DTC.Control.InputText(gg.Code, 12) + "'," +
                    " Leverage = " + (int)gg.Leverage + "," +
                    " GoalGroupOrder = " + gg.Order + "," +
                    " WHERE GoalGroupID = " + gg.ID;
                RunNonQuery(SQL);
            }
            public static GoalInfo GetGoal(DataRow dr, bool getPresentValues)
            {
                GoalInfo goal = new GoalInfo();

                goal.ID = Convert.ToInt32(dr["GoalID"]);
                goal.GroupID = Convert.ToInt16(dr["GoalGroupID"]);
                goal.IsFocus = Convert.ToBoolean(Convert.ToInt16(dr["IsFocus"]));
                goal.TemplateID = Convert.ToInt32(dr["TemplateID"]);
                goal.GoalType = (GoalTypeInfo.TypeEnum) Convert.ToInt16(dr["GoalTypeID"]);
                if (dr["GoalDefinition"] != DBNull.Value)
                    goal.Definition = Convert.ToString(dr["GoalDefinition"]);
                goal.Details = Convert.ToString(dr["Details"]);
                goal.Status = (DTC.StatusEnum)Convert.ToInt16(dr["StatusID"]);
                goal.Nature = (GoalInfo.NatureEnum)Convert.ToInt16(dr["GoalNature"]);
                goal.Range = (DTC.RangeEnum)Convert.ToInt16(dr["GoalRange"]);
                goal.Size = (DTC.SizeEnum)Convert.ToInt16(dr["Size"]);
                goal.OwnerID = Convert.ToInt32(dr["OwnerID"]);
                goal.StartDate = Convert.ToDateTime(dr["DateStart"]);
                goal.DueDate = Convert.ToDateTime(dr["DateDue"]);
                goal.EndDate = Convert.ToDateTime(dr["DateEnd"]);
                goal.StartingValue = Convert.ToSingle(dr["StartingValue"]);
                goal.GoalValue = Convert.ToSingle(dr["GoalValue"]);
                goal.ThresholdValue = Convert.ToSingle(dr["ThresholdValue"]);
                goal.GenericValue = Convert.ToSingle(dr["GenericValue"]);
                goal.EstimatedMinutes = Convert.ToInt16(dr["EstimatedMinutes"]);
                goal.EstimatedHours = Convert.ToSingle(dr["EstimatedHours"]);
                goal.ItemID = Convert.ToInt32(dr["ItemID"]);
                goal.ProjectGroupID = Convert.ToInt32(dr["ProjectGroupID"]);
                goal.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                goal.SecondaryProjectGroupID = Convert.ToInt32(dr["SecondaryProjectGroupID"]);
                goal.SecondaryProjectID = Convert.ToInt32(dr["SecondaryProjectID"]);
                goal.TertiaryProjectGroupID = Convert.ToInt32(dr["TertiaryProjectGroupID"]);
                goal.TertiaryProjectID = Convert.ToInt32(dr["TertiaryProjectID"]);
                goal.Hour = Convert.ToString(dr["Hour"]);
                goal.Criteria = Convert.ToInt16(dr["Criteria"]);
                goal.ItemNature = Convert.ToInt16(dr["ItemNature"]);
                goal.MeasurementStyle = Convert.ToInt16(dr["MeasurementStyle"]);
                goal.PragmaID = Convert.ToInt32(dr["PragmaID"]);
                goal.PragmaAttributeID = Convert.ToInt32(dr["PragmaAttributeID"]);
                goal.PragmaAttributePhrase = Convert.ToString(dr["PragmaAttributePhrase"]);
                goal.PragmaAttributeValue = Convert.ToSingle(dr["PragmaAttributeValue"]);
                goal.PragmaNumInstances = Convert.ToInt16(dr["PragmaNumInstances"]);
                goal.IsBlackAndWhite = Convert.ToBoolean(dr["IsBlackAndWhite"]);
                goal.PrimaryProjectID = Convert.ToInt32(dr["PrimaryProjectID"]);
                goal.HoursPerUnit = Convert.ToSingle(dr["HoursPerUnit"]);

                if (getPresentValues) goal.SetPresentValue(GetPresentValue(goal, goal.EndDate));

                return goal;
            }
            public static long AddGoal(GoalInfo goal)
            {
                string SQL = "SET NOCOUNT ON INSERT INTO Goals (" +
                    " GoalGroupID, IsFocus, TemplateID, GoalTypeID, GoalDefinition, Details, " +
                    " StatusID, GoalNature, GoalRange, Size, OwnerID, DateStart, DateDue, DateEnd," +
                    " StartingValue, GoalValue, ThresholdValue, GenericValue, PrimaryProjectID," +
                    " EstimatedMinutes, EstimatedHours, " +
                    " ItemID, ProjectGroupID, ProjectID, SecondaryProjectGroupID, SecondaryProjectID," +
                    " TertiaryProjectGroupID, TertiaryProjectID, Hour, Criteria, ItemNature, MeasurementStyle," +
                    " PragmaID, PragmaAttributeID, PragmaAttributePhrase, " +
                    " PragmaAttributeValue, PragmaNumInstances, IsBlackAndWhite, HoursPerUnit" +
                    " )" +
                    " VALUES (" +
                    goal.GroupID + "," +
                    Convert.ToInt16(goal.IsFocus) + "," +
                    goal.TemplateID + "," +
                    (int)goal.GoalType + "," +
                    "'" + DTC.Control.InputText(goal.Definition, 255) + "'," +
                    "'" + DTC.Control.InputText(goal.Details, 255) + "'," +
                    (int)goal.Status + "," +
                    (int)goal.Nature + "," +
                    (int)goal.Range + "," +
                    (int)goal.Size + "," +
                    goal.OwnerID + "," +
                    DTC.Date.ObtainGoodDT(goal.StartDate, true) + "," +
                    DTC.Date.ObtainGoodDT(goal.DueDate, true) + "," +
                    DTC.Date.ObtainGoodDT(goal.EndDate, true) + "," +
                    DTC.Control.CommaToDot(goal.StartingValue) + "," +
                    DTC.Control.CommaToDot(goal.GoalValue) + "," +
                    DTC.Control.CommaToDot(goal.ThresholdValue) + "," +
                    DTC.Control.CommaToDot(goal.GenericValue) + "," +
                    goal.PrimaryProjectID + "," +
                    goal.EstimatedMinutes + "," +
                    DTC.Control.CommaToDot(goal.EstimatedHours) + "," +
                    goal.ItemID + "," +
                    goal.ProjectGroupID + "," +
                    goal.ProjectID + "," +
                    goal.SecondaryProjectGroupID + "," +
                    goal.SecondaryProjectID + "," +
                    goal.TertiaryProjectGroupID + "," +
                    goal.TertiaryProjectID + "," +
                    "'" + goal.Hour + "'," +
                    goal.Criteria + "," +
                    goal.ItemNature + "," +
                    goal.MeasurementStyle + "," +
                    goal.PragmaID + "," +
                    goal.PragmaAttributeID + "," +
                    "'" + goal.PragmaAttributePhrase + "'," +
                    DTC.Control.CommaToDot(goal.PragmaAttributeValue) + "," +
                    goal.PragmaNumInstances + "," +
                    Convert.ToInt16(goal.IsBlackAndWhite) + "," +
                    goal.HoursPerUnit +
                    " ) SELECT SCOPE_IDENTITY() AS GoalID";
                goal.ID = RunExecuteScalar(SQL);

                return goal.ID;
            }
            public static long AddSegmentGoal(long segmentID, DateTime theDateOfTheDay)
            {
                DayInfo day = Days.GetDay(theDateOfTheDay, true);
                SegmentInfo segment = Segments.GetSegment(segmentID);
                BlockInfo block = Blocks.GetBlock(segment.BlockID);

                if(day != null && segment != null && block != null)
                {
                    GoalInfo goal = new GoalInfo()
                    {
                        GroupID = 2,     // TODO: Get it from DB or enum
                        GoalType = GoalTypeInfo.TypeEnum.Segment,
                        Definition = segment.Title,
                        Range = DTC.RangeEnum.Day,
                        Nature = GoalInfo.NatureEnum.Standart,
                        StartDate = day.StartInstance,
                        EndDate = day.EndInstance,
                        Size = DTC.SizeEnum.Medium,
                        Status = DTC.StatusEnum.Running,
                        OwnerID = day.DayID,
                        StartingValue = 0,
                        GoalValue = 100,
                        ItemID = segment.ID,
                        HoursPerUnit = 1,
                        PrimaryProjectID = block.ProjectID
                    };
                    
                    // For Segment type of goals we always create a Task
                    TaskInfo task = DTC.SegmentsBlocksTasks.GetTaskFromSegment(segment, theDateOfTheDay);
                    Tasks.AddUpdateTask(task);

                    return AddGoal(goal);
                }
                else
                {
                    return 0;
                }
            }
            public static bool CompleteSegmentGoal(long goalID)
            {
                GoalInfo goal = DB.Goals.GetGoal(goalID, true);
                SegmentInfo segment = DB.GetSegment(goal.ItemID);

                if(goal != null && segment != null)
                {
                    goal.EndDate = DateTime.Today;

                    List<TaskInfo> tasks = Tasks.GetTasksOfSegment(segment.ID, TaskStatusEnum.Completed);
                    if(tasks !=null && tasks.Count > 0)
                        goal.EndDate = tasks.First().TaskDate;

                    goal.Status = DTC.StatusEnum.Success;
                    
                    DB.Goals.UpdateGoal(goal);

                    segment.Status = DTC.StatusEnum.Success;
                    segment.EndDate = goal.EndDate;
                    DB.Segments.AddUpdateSegment(segment);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static bool CompleteGoal(long goalID)
            {
                GoalInfo goal = DB.Goals.GetGoal(goalID, true);
                bool result = false;
                
                if (goal != null)
                {
                    if(goal.GoalType == GoalTypeInfo.TypeEnum.Segment)
                    {
                        SegmentInfo segment = DB.GetSegment(goal.ItemID);
                        if (segment != null)
                        {
                            goal.EndDate = DateTime.Today;
                            List<TaskInfo> tasks = Tasks.GetTasksOfSegment(segment.ID, TaskStatusEnum.Completed);
                            if (tasks != null && tasks.Count > 0)
                                goal.EndDate = tasks.First().TaskDate;

                            segment.Status = DTC.StatusEnum.Success;
                            segment.EndDate = goal.EndDate;
                            DB.Segments.AddUpdateSegment(segment);

                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    if(goal.GoalType == GoalTypeInfo.TypeEnum.Block)
                    {
                        // TODO: Burası için ayrı mekanizma yapıalcak
                    }
                    else
                    {
                        goal.EndDate = goal.DueDate;
                        result = true;
                    }

                    if(result == true)
                    {
                        goal.Status = DTC.StatusEnum.Success;
                        DB.Goals.UpdateGoal(goal);
                    }
                }

                return result;
            }
            public static void UpdateGoal(GoalInfo goal)
            {
                string SQL = "UPDATE Goals SET " +
                     " GoalGroupID = " + goal.GroupID + "," +
                     " IsFocus = " + Convert.ToInt16(goal.IsFocus) + "," +
                     " GoalTypeID = " + (int)goal.GoalType + "," +
                     " GoalDefinition = '" + DTC.Control.InputText(goal.Definition, 255) + "'," +
                     " Details = '" + DTC.Control.InputText(goal.Details, 255) + "'," +
                     " StatusID = " + (int)goal.Status + "," +
                     " GoalNature = " + (int)goal.Nature + "," +
                     " GoalRange = " + (int)goal.Range + "," +
                     " Size = " + (int)goal.Size + "," +
                     " OwnerID = " + goal.OwnerID + "," +
                     " DateStart = " + DTC.Date.ObtainGoodDT(goal.StartDate, true) + "," +
                     " DateDue = " + DTC.Date.ObtainGoodDT(goal.DueDate, true) + "," +
                     " DateEnd = " + DTC.Date.ObtainGoodDT(goal.EndDate, true) + "," +
                     " StartingValue = " + DTC.Control.CommaToDot(goal.StartingValue) + "," +
                     " GoalValue = " + DTC.Control.CommaToDot(goal.GoalValue) + "," +
                     " ThresholdValue = " + DTC.Control.CommaToDot(goal.ThresholdValue) + "," +
                     " GenericValue = " + DTC.Control.CommaToDot(goal.GenericValue) + "," +
                     " PrimaryProjectID = " + goal.PrimaryProjectID + "," +
                     " EstimatedMinutes = " + goal.EstimatedMinutes + "," +
                     " EstimatedHours = " + DTC.Control.CommaToDot(goal.EstimatedHours) + "," +
                     " ItemID = " + goal.ItemID + "," +
                     " ProjectGroupID = " + goal.ProjectGroupID + "," +
                     " ProjectID = " + goal.ProjectID + "," +
                     " SecondaryProjectGroupID = " + goal.SecondaryProjectGroupID + "," +
                     " SecondaryProjectID = " + goal.SecondaryProjectID + "," +
                     " TertiaryProjectGroupID = " + goal.TertiaryProjectGroupID + "," +
                     " TertiaryProjectID = " + goal.TertiaryProjectID + "," +
                     " Hour = '" + goal.Hour + "'," +
                     " Criteria = " + goal.Criteria + "," +
                     " ItemNature = " + goal.ItemNature + "," +
                     " MeasurementStyle = " + goal.MeasurementStyle + "," +
                     " PragmaID = " + goal.PragmaID + "," +
                     " PragmaAttributeID = " + goal.PragmaAttributeID + "," +
                     " PragmaAttributePhrase = '" + goal.PragmaAttributePhrase + "'," +
                     " PragmaAttributeValue = " + DTC.Control.CommaToDot(goal.PragmaAttributeValue) + "," +
                     " PragmaNumInstances = " + goal.PragmaNumInstances + "," +
                     " IsBlackAndWhite = " + Convert.ToInt16(goal.IsBlackAndWhite) + "," +
                     " HoursPerUnit = " + Convert.ToSingle(goal.HoursPerUnit) +
                     " WHERE GoalID = " + goal.ID;
                RunNonQuery(SQL);
            }

            public static List<GoalGroupInfo> GetGoalGroups()
            {
                List<GoalGroupInfo> data = new List<GoalGroupInfo>();
                string SQL = "SELECT * " +
                   " FROM GoalGroups " +
                   " ORDER BY GoalGroupOrder";
                DataTable dt = RunExecuteReader(SQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetGoalGroup(dr));

                return data;
            }
            public static List<GoalTypeInfo> GetGoalTypes()
            {
                List<GoalTypeInfo> data = new List<GoalTypeInfo>();

                string SQL = "SELECT * " +
                     " FROM GoalTypes " +
                     " ORDER BY GoalTypeOrder";
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                {
                    GoalTypeInfo info = new GoalTypeInfo();

                    info.ID = Convert.ToInt32(dr["GoalTypeID"]);
                    info.Type = (GoalTypeInfo.TypeEnum)info.ID;
                    info.Order = Convert.ToInt32(dr["GoalTypeOrder"]);
                    info.Name = Convert.ToString(dr["GoalTypeName"]);
                    info.Code = Convert.ToString(dr["GoalTypeCode"]);
                    info.Nature = (GoalTypeInfo.NatureEnum)Convert.ToInt32(dr["GoalNature"]);
                    info.IsObjective = Convert.ToBoolean(dr["IsObjective"]);

                    data.Add(info);
                }
                
                if (!data.Exists(i=>i.Type == GoalTypeInfo.TypeEnum.NA))
                {
                    GoalTypeInfo info = new GoalTypeInfo();

                    info.ID = 0;
                    info.Type = GoalTypeInfo.TypeEnum.NA;
                    info.Order = 0;
                    info.Name = GoalTypeInfo.TypeEnum.NA.ToString();
                    info.Code = GoalTypeInfo.TypeEnum.NA.ToString();

                    data.Add(info);
                }


                return data;
            }
            public static List<GoalInfo> GetGoals(DTC.RangeEnum range, DateTime startDate, DateTime endDate, bool getPresentValues)
            {
                string SQL = "SELECT * " +
                    " FROM Goals " +
                    " WHERE TemplateID = 0" +
                    " AND GoalRange =" + (int)range +
                    " AND (" +
                    " (DateStart >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    "  AND DateStart <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ) OR " +
                    " (DateDue >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND DateDue <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ) )" +
                    " ORDER BY DateDue";
                return GetGoals(SQL, getPresentValues);
            }
            public static List<GoalInfo> GetGoalsOfProject(int projectID, bool isOnlyRunningOnes, bool getPresentValues)
            {
                string strIsOnlyRunninOnesProject = string.Empty;
                string strIsOnlyRunninOnesBlock = string.Empty;
                string strIsOnlyRunninOnesSegment = string.Empty;

                ProjectInfo project = Projects.GetProject(projectID);

                if(isOnlyRunningOnes)
                {
                    strIsOnlyRunninOnesProject = " AND StatusID =" + (int)DTC.StatusEnum.Running;
                    strIsOnlyRunninOnesBlock = " AND Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Block;
                    strIsOnlyRunninOnesSegment = " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running;
                }

                List<GoalInfo> data = new List<GoalInfo>();
                string SQL = "SELECT * " +
                    " FROM Goals " +
                    " WHERE ProjectID = " + projectID +
                    strIsOnlyRunninOnesProject +
                    " AND TemplateID = 0" +
                    " ORDER BY DateDue";
                foreach (GoalInfo g in GetGoals(SQL, getPresentValues))
                    data.Add(g);

                SQL = "SELECT * " +
                    " FROM Goals " +
                    " WHERE ProjectGroupID = " + project.ProjectGroupID +
                    strIsOnlyRunninOnesProject +
                    " AND TemplateID = 0" +
                    " ORDER BY DateDue";
                foreach (GoalInfo g in GetGoals(SQL, getPresentValues))
                    data.Add(g);

                SQL = "SELECT * " +
                    " FROM Goals " +
                    " WHERE (PrimaryProjectID = " + projectID + " OR " +
                    " SecondaryProjectID = " + projectID + " OR " +
                    " TertiaryProjectID = " + projectID + ") " + 
                    " AND GoalTypeID <> " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " AND GoalTypeID <> " + (int)GoalTypeInfo.TypeEnum.Block +
                    strIsOnlyRunninOnesProject +
                    " AND TemplateID = 0" +
                    " ORDER BY DateDue";
                foreach (GoalInfo g in GetGoals(SQL, getPresentValues))
                    data.Add(g);

                SQL = "SELECT Goals.*" +
                    " FROM Goals " +
                    " INNER JOIN Blocks ON Goals.ItemID = Blocks.BlockID" +
                    " WHERE Blocks.ProjectID = " + projectID +
                    strIsOnlyRunninOnesBlock;
                foreach (GoalInfo g in GetGoals(SQL, getPresentValues))
                    data.Add(g);

                SQL = "SELECT Goals.*" +
                    " FROM  Segments " +
                    " INNER JOIN Blocks ON Segments.BlockID = Blocks.BlockID " +
                    " INNER JOIN Goals ON Segments.SegmentID = Goals.ItemID" +
                    " WHERE Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    strIsOnlyRunninOnesSegment +
                    " AND Blocks.ProjectID = " + projectID;
                foreach (GoalInfo g in GetGoals(SQL, getPresentValues))
                    data.Add(g);

                return data;
            }
            public static List<GoalInfo> GetGoalsOfSegments()
            {
                List<GoalInfo> data = new List<GoalInfo>();

                DateTime dtEnd = DateTime.Today.AddDays(5);
                DateTime dtStart = dtEnd.AddDays(-7);

                string SQL = "SELECT *" +
                    " FROM  Goals " +
                    " WHERE GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " AND StatusID = " + (int)DTC.StatusEnum.Running +
                    " AND DateStart >= " + DTC.ObtainGoodDT(dtStart, true) +
                    " AND DateStart <= " + DTC.ObtainGoodDT(dtEnd, true);

                DataTable dt = RunExecuteReader(SQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetGoal(dr, false));

                return data;
            }
            public static List<GoalInfo> GetGoalsOfSegments(int projectID, bool isOnlyRunningOnes, bool getPresentValues)
            {
                List<GoalInfo> data = new List<GoalInfo>();
                string strIsOnlyRunningOnes = string.Empty;

                if (isOnlyRunningOnes)
                    strIsOnlyRunningOnes = " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running;
 
                string SQL = "SELECT Goals.*" +
                    " FROM  Segments " +
                    " INNER JOIN Blocks ON Segments.BlockID = Blocks.BlockID " +
                    " INNER JOIN Goals ON Segments.SegmentID = Goals.ItemID" +
                    " WHERE Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    strIsOnlyRunningOnes +
                    " AND Blocks.ProjectID = " + projectID;
                foreach (GoalInfo g in GetGoals(SQL, getPresentValues))
                    data.Add(g);

                return data;
            }
            public static List<GoalInfo> GetGoalsOfSegments(DateTime dateStart, DateTime dateEnd)
            {
                List<GoalInfo> data = new List<GoalInfo>();
                
                string SQL = "SELECT Goals.*" +
                    " FROM  Segments " +
                    " INNER JOIN Goals ON Segments.SegmentID = Goals.ItemID" +
                    " WHERE Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running +
                    " AND Goals.DateStart >= " + DTC.ObtainGoodDT(dateStart, true) +
                    " AND Goals.DateEnd <= " + DTC.ObtainGoodDT(dateEnd, true); 

                foreach (GoalInfo g in GetGoals(SQL, false))
                    data.Add(g);

                return data;
            }
            public static List<GoalInfo> GetSegmentGoalsOfBlock(int blockID, bool isOnlyRunningOnes, bool getPresentValues)
            {
                List<GoalInfo> data = new List<GoalInfo>();
                string strIsOnlyRunningOnes = string.Empty;

                if (isOnlyRunningOnes)
                    strIsOnlyRunningOnes = " AND Goals.StatusID = " + (int)DTC.StatusEnum.Running;

                string SQL = "SELECT Goals.* " +
                    " FROM  Segments " +
                    " INNER JOIN Goals ON Segments.SegmentID = Goals.ItemID" +
                    " WHERE Goals.GoalTypeID = " + (int)GoalTypeInfo.TypeEnum.Segment +
                    strIsOnlyRunningOnes +
                    " AND Segments.BlockID = " + blockID;
                foreach (GoalInfo g in GetGoals(SQL, getPresentValues))
                    data.Add(g);

                return data;
            }
            public static List<GoalInfo> GetGoals(GoalTypeInfo.TypeEnum goalType, DateTime startDate, DateTime endDate, bool getPresentValues)
            {
                string SQL = "SELECT * " +
                    " FROM Goals" +
                    " WHERE TemplateID = 0" +
                    " AND GoalTypeID = " + (int)goalType +
                    " AND (" +
                    " (DateStart >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    "  AND DateStart <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ) OR " +
                    " (DateDue >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND DateDue <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ) )" +
                    " ORDER BY DateDue";
                return GetGoals(SQL, getPresentValues);
            }
            public static List<GoalInfo> GetGoals(GoalTypeInfo.TypeEnum goalType, DTC.RangeEnum range, DateTime startDate, DateTime endDate, bool getPresentValues)
            {
                return GetGoals(goalType, startDate, endDate, getPresentValues).FindAll(i => i.Range == range);
            }
            public static List<GoalInfo> GetImportantGoals(DTC.RangeEnum range, DateTime startDate, DateTime endDate, bool getPresentValues)
            {
                return GetGoals(range, startDate, endDate, getPresentValues).FindAll(i => i.Range == range && i.GoalType == GoalTypeInfo.TypeEnum.ProjectGoal || i.GoalType == GoalTypeInfo.TypeEnum.Block || i.IsFocus);
            }
            public static List<GoalInfo> GetGoals(OwnerInfo owner, bool getPresentValues)
            {
                string SQL = "SELECT * " +
                    " FROM Goals " +
                    " WHERE TemplateID = 0" +
                    " AND OwnerID =" + owner.OwnerID +
                    " AND GoalRange = " + (int)owner.Range +
                    " ORDER BY DateDue";
                return GetGoals(SQL, getPresentValues);
            }
            public static List<GoalInfo> GetGoals(string SQL, bool getPresentValues)
            {
                List<GoalInfo> data = new List<GoalInfo>();
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                    data.Add(GetGoal(dr, getPresentValues));

                return data;
            }
            public static List<GoalInfo> GetGoals(List<OwnerInfo> owners, bool getPresentValues)
            {
                string SQL = "SELECT * FROM Goals " +
                    " WHERE TemplateID = 0 " +
                    " AND GoalID < 0";
                foreach (OwnerInfo owner in owners)
                    SQL += " OR (OwnerID =" + owner.OwnerID + " AND GoalRange = " + (int)owner.Range + ")";

                SQL += " ORDER BY GoalRange, DateDue";

                return GetGoals(SQL, getPresentValues);
            }
            public static List<GoalInfo> GetGoalsWithFocus(DTC.RangeEnum range)
            {
                string strRange = "";
                if (range != DTC.RangeEnum.Floating)
                {
                    strRange = " AND Range = " + (int)range;
                }
                string SQL = "SELECT * " +
                    " FROM Goals " +
                    " WHERE TemplateID = 0" +
                    " AND IsFocus = 1" +
                    " AND StatusID = " + (int)DTC.StatusEnum.Running +
                    strRange +
                    " ORDER BY DateDue";
                return GetGoals(SQL, true);
            }
            public static GoalInfo GetGoal(long goalID, bool getPresentValues)
            {
                GoalInfo goal = new GoalInfo();

                string SQL = "SELECT * " +
                    " FROM Goals" +
                    " WHERE GoalID = " + goalID;
                DataTable dt = RunExecuteReader(SQL);

                foreach (DataRow dr in dt.Rows)
                    goal = GetGoal(dr, getPresentValues);
                
                return goal;
            }
            public static bool DeleteGoal(long goalID)
            {
                string SQL;

                SQL = "DELETE Goals WHERE GoalID = " + goalID;
                RunNonQuery(SQL);

                return true;
            }

            /// <summary>
            /// This is a core method. Evaluates the present situation of a goal.
            /// The method should be replaced with a more clever approach.
            /// </summary>
            /// <param name="goal">GoalInfo object.</param>
            /// <returns>A floating point value of the present value of the goal.</returns>
            public static float GetPresentValue(GoalInfo goal, DateTime cutOffDate)
            {
                string SQL = "";
                float result = 0;

                #region TotalHours
                if (goal.GoalType == GoalTypeInfo.TypeEnum.TotalHours)
                {
                    string sqlProjectType = "";
                    if (goal.ItemID > 0)
                    {
                        sqlProjectType = " AND ProjectGroups.ProjectTypeID = " + goal.ItemID;
                    }

                    // standart tasks
                    SQL = "SELECT CONVERT(float, SUM(Tasks.RealTime)) / 60 AS TOTAL" +
                        " FROM Tasks INNER JOIN" +
                        " ProjectGroups ON Tasks.ProjectGroupID = ProjectGroups.ProjectGroupID" +
                        GetDateFilterSQL(goal, "Tasks") + sqlProjectType;
                    DataTable dt = RunExecuteReader(SQL);

                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[0] != DBNull.Value)
                            result = Convert.ToSingle(dr[0]);
                    }

                    // co-tasks
                    //SQL = "SELECT CONVERT(float, SUM(Tasks.RealTime)) / 60 AS TOTAL FROM Tasks" +
                    //    " INNER JOIN CoTasks ON Tasks.TaskID = CoTasks.TaskID" +
                    //    GetCommonTaskSQL(goal, "Tasks") + CollectProjectFields(goal, "CoTasks");
                    SQL = "SELECT CONVERT(float, SUM(Tasks.RealTime)) / 60 AS TOTAL" +
                        " FROM Tasks " +
                        " INNER JOIN CoTasks ON Tasks.TaskID = CoTasks.TaskID " +
                        " INNER JOIN ProjectGroups ON CoTasks.ProjectGroupID = ProjectGroups.ProjectGroupID" +
                        GetDateFilterSQL(goal, "Tasks") + sqlProjectType;
                    dt = RunExecuteReader(SQL);

                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[0] != DBNull.Value)
                            result += Convert.ToSingle(dr[0]);
                    }
                    
                    SQL = "";
                }
                #endregion
                #region TotalLeverage
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.TotalLeverage)
                {
                    // standart tasks
                    SQL = "SELECT CONVERT(float, SUM(Tasks.Leverage * Tasks.RealTime)) / 60 AS TOTAL FROM Tasks" +
                        GetDateFilterSQL(goal, "Tasks") + CollectProjectFields(goal, "Tasks");
                    DataTable dt = RunExecuteReader(SQL);

                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[0] != DBNull.Value)
                            result = Convert.ToSingle(dr[0]);
                    }

                    // co-tasks
                    SQL = "SELECT CONVERT(float, SUM(CoTasks.Leverage * Tasks.RealTime)) / 60 AS TOTAL FROM Tasks" +
                        " INNER JOIN CoTasks ON Tasks.TaskID = CoTasks.TaskID" +
                        GetDateFilterSQL(goal, "Tasks") + CollectProjectFields(goal, "CoTasks");
                    dt = RunExecuteReader(SQL);
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[0] != DBNull.Value)
                            result += Convert.ToSingle(dr[0]);
                    }
                    

                    SQL = "";
                }
                #endregion
                #region NumberOfProjectInstances
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfProjectInstances)
                {
                    // standart tasks
                    SQL = "SELECT COUNT(Tasks.TaskID) AS TOTAL FROM Tasks" +
                        GetDateFilterSQL(goal, "Tasks") + CollectProjectFields(goal, "Tasks") +
                        " AND Tasks.IsCompleted = 1";
                    DataTable dt = RunExecuteReader(SQL);

                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[0] != DBNull.Value)
                            result = Convert.ToSingle(dr[0]);
                    }

                    // co-tasks
                    SQL = "SELECT COUNT(Tasks.TaskID) AS TOTAL FROM Tasks" +
                        " INNER JOIN CoTasks ON Tasks.TaskID = CoTasks.TaskID" +
                        GetDateFilterSQL(goal, "Tasks") + CollectProjectFields(goal, "CoTasks") +
                        " AND Tasks.IsCompleted = 1";
                    dt = RunExecuteReader(SQL);
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[0] != DBNull.Value)
                            result += Convert.ToSingle(dr[0]);
                    }
                    

                    SQL = "";
                }
                #endregion
                #region Total Project Hours
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.TotalProjectHours)
                {
                    // standart tasks
                    SQL = "SELECT CONVERT(float, SUM(Tasks.RealTime)) / 60 AS TOTAL FROM Tasks" +
                        GetDateFilterSQL(goal, "Tasks") + CollectProjectFields(goal, "Tasks");
                    DataTable dt = RunExecuteReader(SQL);
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[0] != DBNull.Value)
                            result = Convert.ToSingle(dr[0]);
                    }

                    // co-tasks
                    SQL = "SELECT CONVERT(float, SUM(Tasks.RealTime)) / 60 AS TOTAL FROM Tasks" +
                        " INNER JOIN CoTasks ON Tasks.TaskID = CoTasks.TaskID" +
                        GetDateFilterSQL(goal, "Tasks") + CollectProjectFields(goal, "CoTasks");
                    dt = RunExecuteReader(SQL);
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[0] != DBNull.Value)
                            result += Convert.ToSingle(dr[0]);
                    }
                    

                    SQL = "";
                }
                #endregion
                #region NumberOfIdeas
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfIdeas)
                {
                    SQL = "SELECT COUNT(Ideas.IdeaID) AS TOTAL" +
                        " FROM Ideas INNER JOIN IdeaGroups ON Ideas.IdeaGroupID = IdeaGroups.IdeaGroupID" +
                        " WHERE Ideas.CreationDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                        " AND Ideas.CreationDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                        CollectProjectFields(goal, "IdeaGroups");
                }
                #endregion
                #region Number of Books
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfBooks)
                {
                    SQL = "SELECT COUNT(BookID) AS TOTAL FROM Books " +
                        " WHERE EndDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                        " AND EndDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                        " AND Nature = " + goal.ItemNature +
                        " AND Status = " + (int)DTC.StatusEnum.Success +
                        CollectProjectFields(goal, "Books");
                }
                #endregion
                #region Number of Todos
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfTodos)
                {
                    SQL = "SELECT COUNT(Tasks.TaskID) AS TOTAL" +
                        " FROM Tasks" +
                        " WHERE Tasks.IsCompleted = 1 " +
                        " AND Tasks.IsThing = 1 " +
                        " AND Tasks.TaskDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                        " AND Tasks.TaskDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                        CollectProjectFields(goal, "Tasks");
                }
                #endregion
                #region NumberOfSegments
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfSegments)
                {
                    SQL = "SELECT COUNT(Segments.SegmentID) AS TOTAL " +
                        " FROM Segments INNER JOIN  Blocks ON Segments.BlockID = Blocks.BlockID " +
                        " WHERE Segments.StatusID = " + (int)DTC.StatusEnum.Success +
                        " AND Segments.EndDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                        " AND Segments.EndDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                        CollectProjectFields(goal, "Blocks");
                }
                #endregion
                #region NumberOfBlocks
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfBlocks)
                {
                    SQL = "SELECT COUNT(Blocks.BlockID) AS TOTAL " +
                        " FROM Blocks " +
                        " WHERE Blocks.StatusID = " + (int)DTC.StatusEnum.Success +
                        " AND Blocks.EndDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                        " AND Blocks.EndDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                        CollectProjectFields(goal, "Blocks");
                }
                #endregion
                #region NumberOfThings - OBSOLETE
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfThings)
                {
                    SQL = "SELECT COUNT(Tasks.TaskID) AS TOTAL" +
                        " FROM Tasks" +
                        GetDateFilterSQL(goal, "Tasks") +
                        " AND Tasks.IsCompleted = 1" +
                        " AND Tasks.IsThing = 1 " +
                        CollectProjectFields(goal, "Tasks");
                }
                #endregion
                #region Todo Goal
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Todo)
                {
                    SQL = "SELECT COUNT(TaskID) AS TOTAL" +
                        " FROM Tasks WHERE IsCompleted = 1 " +
                        " AND TaskID = " + goal.ItemID;
                }
                #endregion
                #region Weight
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Weight)
                {
                    GoalInfo.MeasurementStyleEnum measurementStyle = GoalInfo.MeasurementStyleEnum.LastValue;
                    if (goal.MeasurementStyle == (int)GoalInfo.MeasurementStyleEnum.Average)
                        measurementStyle = GoalInfo.MeasurementStyleEnum.Average;

                    if (measurementStyle == GoalInfo.MeasurementStyleEnum.LastValue)
                    {
                        SQL = "SELECT TOP 1 TheWeight FROM days" +
                            " WHERE TheWeight > 0" +
                            " AND TheDay >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                            " AND TheDay <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                            " ORDER BY TheDay DESC";
                    }
                    else if (measurementStyle == GoalInfo.MeasurementStyleEnum.Average)
                    {
                        SQL = "SELECT AVG(TheWeight) AS AVGWEIGHT" +
                            " FROM Days " +
                            " WHERE TheWeight > 0" +
                            " AND TheDay >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                            " AND TheDay <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true);
                    }
                }
                #endregion
                #region Pragma
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Pragma)
                {
                    string sqlPhrase = "";
                    if (goal.PragmaAttributePhrase != "")
                    {
                        sqlPhrase = " AND PragmaLog.PragmaAttributeValue = '" + goal.PragmaAttributePhrase + "'";
                    }

                    if (goal.PragmaNumInstances > 0)
                    {
                        SQL = "SELECT COUNT(PragmaInstances.PragmaInstanceID) AS TOTAL" +
                          " FROM PragmaInstances" +
                          " INNER JOIN PragmaAttributes ON PragmaInstances.PragmaID = PragmaAttributes.PragmaID" +
                          " INNER JOIN PragmaLog ON PragmaInstances.PragmaInstanceID = PragmaLog.PragmaInstanceID" +
                          " AND PragmaAttributes.PragmaAttributeID = PragmaLog.PragmaAttributeID" +
                          " WHERE PragmaInstances.PragmaID = " + goal.PragmaID +
                          " AND PragmaAttributes.PragmaAttributeID = " + goal.PragmaAttributeID +
                          " AND PragmaInstances.PragmaInstanceDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                          " AND PragmaInstances.PragmaInstanceDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                          sqlPhrase;
                    }
                    else if (goal.PragmaAttributeValue > 0)
                    {
                        SQL = "SELECT SUM(CONVERT(FLOAT, PragmaLog.PragmaAttributeValue)) AS TOTAL" +
                            " FROM PragmaInstances" +
                            " INNER JOIN PragmaAttributes ON PragmaInstances.PragmaID = PragmaAttributes.PragmaID" +
                            " INNER JOIN PragmaLog ON PragmaInstances.PragmaInstanceID = PragmaLog.PragmaInstanceID" +
                            " AND PragmaAttributes.PragmaAttributeID = PragmaLog.PragmaAttributeID" +
                            " WHERE PragmaInstances.PragmaID = " + goal.PragmaID +
                            " AND PragmaAttributes.PragmaAttributeID = " + goal.PragmaAttributeID +
                            " AND PragmaInstances.PragmaInstanceDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                            " AND PragmaInstances.PragmaInstanceDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                            sqlPhrase;
                    }
                }
                #endregion
                #region Wake Up
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.WakeUp)
                {
                    SQL = "SELECT COUNT(TheDay) AS TOTAL " +
                        " FROM Days" +
                        " WHERE TheDay >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                        " AND TheDay <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                        " AND { fn HOUR(StartInstance) } + CONVERT(float, { fn MINUTE(StartInstance) }) / 60 <= " +
                        DTC.Control.CommaToDot(new TimeInfo(goal.Hour).GetDecimalValue().ToString()) +
                        " AND { fn HOUR(StartInstance) } + CONVERT(float, { fn MINUTE(StartInstance) }) / 60 > 0";
                }
                #endregion
                #region Wake Up Exp
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.WakeUpExp)
                {
                    SQL = "";
                    result = Days.CalculateWakeUpEx(new TimeInfo(goal.Hour), goal.StartDate, goal.DueDate);
                }
                #endregion
                #region Number of Days Over Point
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumDaysOverPoint)
                {
                    SQL = "SELECT COUNT(TheDay) AS TOTAL " +
                        " FROM Days" +
                        " WHERE TheDay >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                        " AND TheDay <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                        " AND Performance >= " + DTC.Control.CommaToDot(goal.ThresholdValue);
                }
                #endregion
                #region Number of Weeks Over Point
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumWeeksOverPoint)
                {
                    SQL = "SELECT COUNT(WeekID) AS TOTAL " +
                        " FROM Weeks" +
                        " WHERE StartDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate.AddDays(-7), true) +
                        " AND StartDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate.AddDays(-7), true) +
                        " AND Performance >= " + DTC.Control.CommaToDot(goal.ThresholdValue);
                }
                #endregion
                #region Number of Months Over Point
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.NumMonthsOverPoint)
                {
                    SQL = "SELECT COUNT(MonthID) AS TOTAL " +
                        " FROM Months" +
                        " WHERE StartDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                        " AND StartDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true) +
                        " AND Performance >= " + DTC.Control.CommaToDot(goal.ThresholdValue);
                }
                #endregion
                #region Book Goal
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Book)
                {
                    SQL = "";
                    result = Books.GetCurrentValue(goal.ItemID);
                }
                #endregion
                #region Project Goal
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.ProjectGoal)
                {
                    SQL = "";
                    ProjectInfo project = Projects.GetProject(goal.ItemID);
                    result = Projects.GetPerformanceFigures(project, true).GetPerformance();
                }
                #endregion
                #region Block
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Block)
                {
                    SQL = "";
                    BlockInfo block = Blocks.GetBlock(goal.ItemID);
                    result = Blocks.GetPerformance(block, cutOffDate);
                }
                #endregion
                #region Segment
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Segment)
                {
                    SQL = "";
                    result = Segments.GetPerformance(Segments.GetSegment(goal.ItemID), goal.EndDate);
                }
                #endregion
                #region Generic Numeric
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.GenericNumeric)
                {
                    SQL = "";
                    result = goal.GenericValue;
                }
                #endregion

                if (SQL != "")
                {
                    DataTable dt = RunExecuteReader(SQL);

                    if(dt.Rows.Count == 1)
                    {
                        if (dt.Rows[0][0] != DBNull.Value)
                            result = Convert.ToSingle(dt.Rows[0][0]);
                    }
                }

                return result;
            }
            /// <summary>
            /// This is a core method. Evaluates the comitted goal value.
            /// </summary>
            /// <param name="goal">GoalInfo object.</param>
            /// <returns>A floating point value of the comitted goal.</returns>
            public static float GetGoalValue(GoalInfo goal)
            {
                float result = 0;

                if (goal.GoalType == GoalTypeInfo.TypeEnum.Verbal || goal.GoalType == GoalTypeInfo.TypeEnum.Todo)
                {
                    result = 1;
                }
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Book)
                {
                    result = Books.GetCurrentValue(goal.ItemID);
                }
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Pragma)
                {
                    if (goal.PragmaNumInstances > 0) result = goal.PragmaNumInstances;
                    else if (goal.PragmaAttributeValue > 0) result = goal.PragmaAttributeValue;
                    else result = 0;
                }
                else
                {
                    result = goal.GoalValue;
                }

                return result;
            }
            public static ProjectInfo GetPrimaryProject(GoalInfo goal)
            {
                ProjectInfo project = new ProjectInfo();

                if (goal.GoalType == GoalTypeInfo.TypeEnum.Book)
                {
                    // IMPLEMENTATION REMOVED
                }
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Todo)
                {
                    TaskInfo task = Tasks.GetTask(goal.ItemID);
                    project = Projects.GetProject(task.ProjectID);
                }
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.TotalProjectHours
                    || goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfProjectInstances
                    || goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfTodos
                    || goal.GoalType == GoalTypeInfo.TypeEnum.TotalLeverage
                    || goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfIdeas
                    || goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfBooks
                    || goal.GoalType == GoalTypeInfo.TypeEnum.NumberOfThings)
                {
                    project = Projects.GetProject(goal.ProjectID);
                }
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.ProjectGoal)
                {
                    project = Projects.GetProject(goal.ItemID);
                }
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Block)
                {
                    BlockInfo block = Blocks.GetBlock(goal.ItemID);
                    project = Projects.GetProject(block.ProjectID);
                }
                else if (goal.GoalType == GoalTypeInfo.TypeEnum.Segment)
                {
                    SegmentInfo segment = Segments.GetSegment(goal.ItemID);
                    BlockInfo block = Blocks.GetBlock(segment.BlockID);
                    project = Projects.GetProject(block.ProjectID);
                }

                return project;
            }

            /// <summary>
            /// The goal type fields have project common tasks. This method enables this.
            /// </summary>
            /// <param name="goal">The goal</param>
            /// <returns>The SQL part containing fields</returns>
            private static string CollectProjectFields(GoalInfo goal, string tableName)
            {
                string SQL = " AND (";
                string tablePart = "";
                bool foundAnything = false;

                if (tableName != "") tablePart = tableName + ".";


                if (goal.ProjectID > 0)
                {
                    SQL += " " + tablePart + "ProjectID = " + goal.ProjectID + " OR ";
                    foundAnything = true;
                }
                else if (goal.ProjectGroupID > 0)
                {
                    SQL += " " + tablePart + "ProjectGroupID = " + goal.ProjectGroupID + " OR ";
                    foundAnything = true;
                }

                if (goal.SecondaryProjectID > 0)
                {
                    SQL += " " + tablePart + "ProjectID = " + goal.SecondaryProjectID + " OR ";
                    foundAnything = true;
                }
                else if (goal.SecondaryProjectGroupID > 0)
                {
                    SQL += " " + tablePart + "ProjectGroupID = " + goal.SecondaryProjectGroupID + " OR ";
                    foundAnything = true;
                }

                if (goal.TertiaryProjectID > 0)
                {
                    SQL += " " + tablePart + "ProjectID = " + goal.TertiaryProjectID + " OR ";
                    foundAnything = true;
                }
                else if (goal.TertiaryProjectGroupID > 0)
                {
                    SQL += " " + tablePart + "ProjectGroupID = " + goal.TertiaryProjectGroupID + " OR ";
                    foundAnything = true;
                }

                SQL += " 0 = 1)";

                if (foundAnything) return SQL;
                else return "";
            }
            /// <summary>
            /// Enables the common part of the SQL, starting from WHERE
            /// </summary>
            /// <param name="goal">Goal</param>
            /// <param name="tableName">Table Name</param>
            /// <returns>Common part of the SQL </returns>
            private static string GetDateFilterSQL(GoalInfo goal, string tableName)
            {
                return " WHERE " + tableName + ".TaskDate >= " + DTC.Date.ObtainGoodDT(goal.StartDate, true) +
                    " AND " + tableName + ".TaskDate <= " + DTC.Date.ObtainGoodDT(goal.DueDate, true);
            }

            public static void RefreshAll(DateTime date)
            {
                /*
                OwnerInfo owner;
                DayInfo day = DB.Days.GetDay(date, true);

                owner = DB.Owner.GetOwner(DTC.RangeEnum.Day, date);
                Dictionary<int, GoalInfo> dailyGoals = DB.Goals.GetGoals(owner, true);
                day.Performance = Convert.ToInt16(DTC.Goals.GetPerformance(dailyGoals, DTC.Goals.PerformanceNatureEnum.Normal) * 100);

                WeekInfo week = DB.Weeks.GetWeek(date, true);
                owner = DB.Owner.GetOwner(DTC.RangeEnum.Week, date);
                Dictionary<int, GoalInfo> weeklyGoals = DB.Goals.GetGoals(owner, true);
                week.Performance = Convert.ToInt16(DTC.Goals.GetPerformance(weeklyGoals, DTC.Goals.PerformanceNatureEnum.Normal) * 100);
                day.PerfWeek = week.Performance;
                DB.Weeks.AddUpdateWeek(week);

                MonthInfo month = DB.Months.GetMonth(date, true);
                owner = DB.Owner.GetOwner(DTC.RangeEnum.Month, date);
                Dictionary<int, GoalInfo> monthlyGoals = DB.Goals.GetGoals(owner, true);
                month.Performance = Convert.ToInt16(DTC.Goals.GetPerformance(monthlyGoals, DTC.Goals.PerformanceNatureEnum.Normal) * 100);
                day.PerfMonth = month.Performance;
                DB.Months.AddUpdateMonth(month);

                QuarterInfo quarter = DB.Quarters.GetQuarter(date, true);
                owner = DB.Owner.GetOwner(DTC.RangeEnum.Quarter, date);
                Dictionary<int, GoalInfo> quarterlyGoals = DB.Goals.GetGoals(owner, true);
                quarter.Performance = Convert.ToInt16(DTC.Goals.GetPerformance(quarterlyGoals, DTC.Goals.PerformanceNatureEnum.Normal) * 100);
                day.PerfQuarter = quarter.Performance;
                DB.Quarters.AddUpdateQuarter(quarter);

                YearInfo year = DB.Years.GetYear(date, true);
                owner = DB.Owner.GetOwner(DTC.RangeEnum.Year, date);
                Dictionary<int, GoalInfo> yearlyGoals = DB.Goals.GetGoals(owner, true);
                year.Performance = Convert.ToInt16(DTC.Goals.GetPerformance(yearlyGoals, DTC.Goals.PerformanceNatureEnum.Normal) * 100);
                day.PerfYear = year.Performance;
                DB.Years.AddUpdateYear(year);

                // we update day on purpose,
                // day has weekly, monthly, quarterly and yearly perf values
                DB.Days.AddUpdateDay(day);

                */
            }
            public static void UpdateGoalAsCompleted(long goalID)
            {
                string strSQL = "UPDATE Goals SET " +
                    " StatusID = " + (int)DTC.StatusEnum.Success +
                    " WHERE GoalID = " + goalID;
                RunNonQuery(strSQL);
            }
            public static void MoveToNextPrevDay(long goalID, int numDays)
            {
                GoalInfo goal = GetGoal(goalID, false);
                DateTime newDayDate = goal.StartDate.AddDays(numDays);
                DayInfo newDay = DB.Days.GetDay(newDayDate, true);

                goal.OwnerID = newDay.DayID;
                goal.StartDate = newDayDate;
                goal.EndDate = newDayDate;
                goal.DueDate = newDayDate;

                UpdateGoal(goal);

                if(goal.GoalType == GoalTypeInfo.TypeEnum.Segment)
                {
                    long segmentID = goal.ItemID;
                    List<TaskInfo> tasks = Tasks.GetTasks("SELECT * FROM Tasks WHERE SegmentID = " + segmentID);
                    foreach(TaskInfo task in tasks) 
                    {
                        task.TaskDate = newDayDate;
                        task.StartDate = newDayDate;
                        task.EndDate = newDayDate;

                        Tasks.AddUpdateTask(task);
                    }
                }
                
            }
        }

        public class GoalTemplates
        {
            public static int AddUpdateGoalTemplate(GoalTemplateInfo goalTemplate)
            {
                string SQL = "";

                if (goalTemplate.ID == 0)
                {
                    SQL = "INSERT INTO GoalTemplates " +
                        " (TemplateName,Range,Details" +
                        " ) VALUES (" +
                        "'" + DTC.Control.InputText(goalTemplate.Name, 50) + "'," +
                        (int)goalTemplate.Range + "," +
                        "'" + DTC.Control.InputText(goalTemplate.Details, 255) + "'" +
                        ") SELECT SCOPE_IDENTITY() AS TemplateID";
                    goalTemplate.ID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE GoalTemplates SET " +
                        " TemplateName = '" + DTC.Control.InputText(goalTemplate.Name, 50) + "'," +
                        " Range = " + (int)goalTemplate.Range + "," +
                        " Details = '" + DTC.Control.InputText(goalTemplate.Details, 255) + "'" +
                        " WHERE TemplateID = " + goalTemplate.ID;
                    RunNonQuery(SQL);
                }

                return goalTemplate.ID;
            }
            public static bool DeleteGoalTemplate(GoalTemplateInfo goalTemplate)
            {
                string SQL = "DELETE Goals WHERE TemplateID = " + goalTemplate.ID;
                RunNonQuery(SQL);

                SQL = "DELETE GoalTemplates WHERE TemplateID = " + goalTemplate.ID;
                RunNonQuery(SQL);

                return true;
            }
            public static GoalTemplateInfo GetGoalTemplate(DataRow dr)
            {
                GoalTemplateInfo info = new GoalTemplateInfo();

                info.ID = Convert.ToInt32(dr["TemplateID"]);
                info.Name = Convert.ToString(dr["TemplateName"]);
                info.Range = (DTC.RangeEnum)Convert.ToInt16(dr["Range"]);
                if (dr["Details"] != DBNull.Value) info.Details = Convert.ToString(dr["Details"]);

                return info;
            }
            public static Dictionary<int, GoalTemplateInfo> GetGoalTemplates(string SQL)
            {
                Dictionary<int, GoalTemplateInfo> dict = new Dictionary<int, GoalTemplateInfo>();

                DataTable dt = RunExecuteReader(SQL);
                foreach (DataRow dr in dt.Rows)
                {
                    dict.Add(Convert.ToInt32(dr["TemplateID"]), GetGoalTemplate(dr));
                }
                
                return dict;
            }
            public static Dictionary<int, GoalTemplateInfo> GetGoalTemplates()
            {
                string SQL = "SELECT * FROM GoalTemplates ORDER BY TemplateName ASC";
                return GetGoalTemplates(SQL);
            }
            public static List<GoalInfo> GetGoals(GoalTemplateInfo goalTemplate)
            {
                string SQL = "SELECT * FROM Goals" +
                    " WHERE TemplateID = " + goalTemplate.ID;
                return Goals.GetGoals(SQL, false);
            }
            public static void DeleteGoalsOfTemplate(GoalTemplateInfo template)
            {
                if (template.ID > 0)
                {
                    string SQL = "DELETE Goals WHERE TemplateID = " + template.ID;
                    RunNonQuery(SQL);
                }
            }
        }

        public class Owner
        {
            public static OwnerInfo GetOwner(DTC.RangeEnum range, int ownerID)
            {
                OwnerInfo owner = new OwnerInfo();

                owner.OwnerID = ownerID;
                owner.Range = range;

                if (range == DTC.RangeEnum.Day)
                {
                    DayInfo day = Days.GetDay(ownerID);

                    owner.StartDate = day.TheDate;
                    owner.EndDate = day.TheDate;
                    owner.NO = day.TheDate.Day;
                    owner.Label = day.GetSmartDate(true);
                }
                else if (range == DTC.RangeEnum.Week)
                {
                    WeekInfo week = Weeks.GetWeek(ownerID);

                    owner.StartDate = week.StartDate;
                    owner.EndDate = week.EndDate;
                    owner.NO = week.WeekNO;
                    owner.Label = "Week " + week.WeekNO + "/ " + week.StartDate.Year;
                }
                else if (range == DTC.RangeEnum.Month)
                {
                    MonthInfo month = Months.GetMonth(ownerID);

                    owner.StartDate = month.StartDate;
                    owner.EndDate = month.EndDate;
                    owner.NO = month.StartDate.Month;
                    owner.Label = month.Name + " / " + month.Year;
                }
                else if (range == DTC.RangeEnum.Quarter)
                {
                    QuarterInfo quarter = Quarters.GetQuarter(ownerID);

                    owner.StartDate = quarter.StartDate;
                    owner.EndDate = quarter.EndDate;
                    owner.NO = (int)quarter.Quarter;
                    owner.Label = quarter.Quarter + " / " + quarter.Year;
                }
                else if (range == DTC.RangeEnum.Year)
                {
                    YearInfo year = Years.GetYear(ownerID);

                    owner.StartDate = year.StartDate;
                    owner.EndDate = year.EndDate;
                    owner.NO = year.Year;
                    owner.Label = year.Year.ToString();
                }
                return owner;
            }
            public static OwnerInfo GetOwner(long ownerID)
            {
                OwnerInfo owner = new OwnerInfo();

           
                return owner;
            }
            public static OwnerInfo GetOwner(DTC.RangeEnum range, DateTime date)
            {
                OwnerInfo owner = new OwnerInfo();

                owner.Range = range;

                if (range == DTC.RangeEnum.Day)
                {
                    DayInfo day = Days.GetDay(date, true);

                    owner.StartDate = day.TheDate;
                    owner.EndDate = day.TheDate;
                    owner.NO = day.TheDate.Day;
                    owner.Label = day.GetSmartDate(true);
                    owner.OwnerID = day.DayID;
                }
                else if (range == DTC.RangeEnum.Week)
                {
                    WeekInfo week = Weeks.GetWeek(date, true);

                    owner.StartDate = week.StartDate;
                    owner.EndDate = week.EndDate;
                    owner.NO = week.WeekNO;
                    owner.Label = "Week " + week.WeekNO + "/ " + week.StartDate.Year;
                    owner.OwnerID = week.WeekID;
                }
                else if (range == DTC.RangeEnum.Month)
                {
                    MonthInfo month = Months.GetMonth(date, true);

                    owner.StartDate = month.StartDate;
                    owner.EndDate = month.EndDate;
                    owner.NO = month.StartDate.Month;
                    owner.Label = month.Name + " / " + month.Year;
                    owner.OwnerID = month.MonthID;
                }
                else if (range == DTC.RangeEnum.Quarter)
                {
                    QuarterInfo quarter = Quarters.GetQuarter(date, true);

                    owner.StartDate = quarter.StartDate;
                    owner.EndDate = quarter.EndDate;
                    owner.NO = (int)quarter.Quarter;
                    owner.Label = quarter.Quarter + " / " + quarter.Year;
                    owner.OwnerID = quarter.QuarterID;
                }
                else if (range == DTC.RangeEnum.Year)
                {
                    YearInfo year = Years.GetYear(date, true);

                    owner.StartDate = year.StartDate;
                    owner.EndDate = year.EndDate;
                    owner.NO = year.Year;
                    owner.Label = year.Label;
                    owner.OwnerID = year.YearID;
                }
                return owner;
            }
            public static int GetOwnerID(DTC.RangeEnum range, DateTime startDate)
            {
                int ownerID = 0;

                if (range == DTC.RangeEnum.Day)
                {
                    DayInfo day = Days.GetDay(startDate, true);
                    ownerID = day.DayID;
                }
                else if (range == DTC.RangeEnum.Week)
                {
                    WeekInfo week = Weeks.GetWeek(startDate, true);
                    ownerID = week.WeekID;
                }
                else if (range == DTC.RangeEnum.Month)
                {
                    MonthInfo month = Months.GetMonth(startDate, true);
                    ownerID = month.MonthID;
                }
                else if (range == DTC.RangeEnum.Quarter)
                {
                    QuarterInfo quarter = Quarters.GetQuarter(startDate, true);
                    ownerID = quarter.QuarterID;
                }
                else if (range == DTC.RangeEnum.Year)
                {
                    YearInfo year = Years.GetYear(startDate, true);
                    ownerID = year.YearID;
                }
                return ownerID;
            }
            public static OwnerInfo GetPrevNextOwner(int prevOrNext, DTC.RangeEnum range, DateTime startDate)
            {
                OwnerInfo owner = new OwnerInfo();
                DateTime theDate = DateTime.Today;
                
                if (range == DTC.RangeEnum.Day)
                    theDate = startDate.AddDays(1 * prevOrNext);
                else if (range == DTC.RangeEnum.Week)
                    theDate = startDate.AddDays(7 * prevOrNext);
                else if (range == DTC.RangeEnum.Month)
                    theDate = startDate.AddMonths(1 * prevOrNext);
                else if (range == DTC.RangeEnum.Quarter)
                    theDate = startDate.AddMonths(3 * prevOrNext);
                else if (range == DTC.RangeEnum.Year)
                    theDate = startDate.AddYears(1 * prevOrNext);

                owner = GetOwner(range, theDate);

                return owner;
            }
        }

        public class Books
        {
            private static BookInfo GetBook(DataRow dr)
            {
                BookInfo info = new BookInfo();

                info.ID = Convert.ToInt32(dr["BookID"]);
                info.Title = Convert.ToString(dr["Title"]);
                info.Author = Convert.ToString(dr["Author"]);
                info.Nature = (DTC.BookNature)(Convert.ToInt32(dr["Nature"]));
                info.TotalValue = Convert.ToSingle(dr["TotalValue"]);
                info.CurrentValue = Convert.ToSingle(dr["CurrentValue"]);
                info.IsTrackProgress = Convert.ToBoolean(dr["IsTrackProgress"]);
                info.StartDate = Convert.ToDateTime(dr["StartDate"]);
                if (dr["EndDate"] != DBNull.Value) info.EndDate = Convert.ToDateTime(dr["EndDate"]);
                info.Status = (DTC.StatusEnum)(Convert.ToInt32(dr["Status"]));
                info.Details = Convert.ToString(dr["Details"]);
                info.AudiobookProcessType = (BookInfo.AudiobookProcessTypeEnum) Convert.ToInt16(dr["AudiobookProcessType"]);

                return info;
            }
            public static BookInfo GetBook(long bookID)
            {
                BookInfo info = new BookInfo();

                string SQL = "SELECT * FROM Books" +
                   " WHERE BookID = " + bookID +
                   " ORDER BY StartDate DESC";
                DataTable dt = RunExecuteReader(SQL);
                if (dt.Rows.Count == 1)
                    info = GetBook(dt.Rows[0]);

                return info;
            }
            public static Dictionary<int, BookInfo> GetBooks(DTC.BookNature nature)
            {
                Dictionary<int, BookInfo> dict = new Dictionary<int, BookInfo>();

                string filterSQL = "";
                if (nature != DTC.BookNature.TypeFree) filterSQL = " WHERE Nature = " + (int)nature;

                string strSQL = "SELECT * FROM Books" +
                    filterSQL +
                    " ORDER BY StartDate DESC";
                DataTable dt = RunExecuteReader(strSQL);

                foreach (DataRow dr in dt.Rows)
                    dict.Add(Convert.ToInt32(dr["BookID"]), GetBook(dr));

                return dict;
            }
            public static Dictionary<int, BookInfo> GetBooks(DTC.BookNature nature, DTC.StatusEnum status)
            {
                Dictionary<int, BookInfo> dict = new Dictionary<int, BookInfo>();

                string filterSQL = "";
                if(status != DTC.StatusEnum.NA)
                    filterSQL = " WHERE Status = " + (int)status;
                else
                    filterSQL = " WHERE Status >= " + (int)status;  // take everything

                if (nature != DTC.BookNature.TypeFree) filterSQL += " AND Nature = " + (int)nature;

                string strSQL = "SELECT * FROM Books" +
                    filterSQL +
                    " ORDER BY StartDate DESC";
                DataTable dt = RunExecuteReader(strSQL);

                foreach (DataRow dr in dt.Rows)
                    dict.Add(Convert.ToInt32(dr["BookID"]), GetBook(dr));

                return dict;
            }
            public static Dictionary<int, BookInfo> GetBooks(DTC.BookNature nature, DTC.StatusEnum status, DateTime startDate, DateTime endDate)
            {
                Dictionary<int, BookInfo> dict = new Dictionary<int, BookInfo>();

                string filterSQL = "";
                filterSQL = " WHERE Status = " + (int)status;
                if (nature != DTC.BookNature.TypeFree) filterSQL += " AND Nature = " + (int)nature;

                string strSQL = "SELECT * FROM Books" +
                    filterSQL +
                    " AND EndDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND EndDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ORDER BY StartDate DESC";
                DataTable dt = RunExecuteReader(strSQL);

                foreach (DataRow dr in dt.Rows)
                    dict.Add(Convert.ToInt32(dr["BookID"]), GetBook(dr));

                return dict;
            }
            public static float GetPercentage(int bookID)
            {
                float result = 0;
                BookInfo book = GetBook(bookID);

                if (book.TotalValue > 0) result = 100 * (float)book.CurrentValue / (float)book.TotalValue;
                else result = 0;

                return result;
            }
            public static float GetCurrentValue(long bookID)
            {
                float result = 0;
                BookInfo book = GetBook(bookID);

                result = book.CurrentValue;

                return result;
            }
            public static int AddUpdateBook(BookInfo book)
            {
                string SQL = "";

                if (book.ID == 0)
                {
                    SQL = "SET NOCOUNT ON INSERT INTO Books " +
                         " (Title, Author, Nature, TotalValue, CurrentValue, IsTrackProgress,  " +
                         " StartDate, EndDate, Status, Details, AudiobookProcessType )" +
                         " VALUES (" +
                         "'" + DTC.Control.InputText(book.Title) + "'," +
                         "'" + DTC.Control.InputText(book.Author) + "'," +
                         (int)book.Nature + "," +
                         book.TotalValue + "," +
                         book.CurrentValue + "," +
                         Convert.ToInt16(book.IsTrackProgress) + "," +
                         DTC.Date.ObtainGoodDT(book.StartDate, true) + "," +
                         DTC.Date.ObtainGoodDT(book.EndDate, true) + "," +
                         (int)book.Status + "," +
                         "'" + DTC.Control.InputText(book.Details) + "'," +
                         (int)book.AudiobookProcessType + 
                         ") SELECT SCOPE_IDENTITY() AS BookID";
                    book.ID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Books SET " +
                       " Title = '" + DTC.Control.InputText(book.Title) + "'," +
                       " Author = '" + DTC.Control.InputText(book.Author) + "'," +
                       " Nature = " + (int)book.Nature + "," +
                       " TotalValue = " + book.TotalValue+ "," +
                       " CurrentValue = " + book.CurrentValue + "," +
                       " IsTrackProgress = " + Convert.ToInt16(book.IsTrackProgress) + "," +
                       " StartDate = " + DTC.Date.ObtainGoodDT(book.StartDate, true) + "," +
                       " EndDate = " + DTC.Date.ObtainGoodDT(book.EndDate, true) + "," +
                       " Status = " + (int)book.Status + "," +
                       " Details = '" + DTC.Control.InputText(book.Details) + "'," +
                       " AudiobookProcessType = " + (int)book.AudiobookProcessType +
                       " WHERE BookID = " + book.ID;
                    RunNonQuery(SQL);
                }

                return book.ID;
            }
            public static void DeleteBook(int bookID)
            {
                string strSQL = "DELETE Books WHERE BookID = " + bookID;
                RunNonQuery(strSQL);
            }
            public static string ReReadBook(int bookID)
            {
                string result = string.Empty;

                BookInfo book = GetBook(bookID);
                book.ID = 0;
                book.Status = DTC.StatusEnum.Running;
                book.StartDate = DateTime.Today;
                book.EndDate = DateTime.Today;
                book.CurrentValue = 0;

                AddUpdateBook(book);

                return result;
            }
        }

        public class News
        {
            public static List<NewsInfo> GetNewsWithoutFocus(DateTime startDate, DateTime endDate, bool isDESC)
            {
                string orderBy = "ASC";
                if (isDESC) orderBy = "DESC";

                string strSQL = "SELECT * FROM News" +
                    " WHERE TheDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TheDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " AND IsWeeklyFocus = 0 " +
                    " AND IsMonthlyFocus = 0 " +
                    " ORDER BY TheDate " + orderBy;
                return GetNews(strSQL);
            }
            public static List<NewsInfo> GetWeeklyFocus(DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT * FROM News" +
                    " WHERE TheDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TheDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " AND IsWeeklyFocus = 1 " +
                    " AND IsMonthlyFocus = 0 " +
                    " ORDER BY TheDate DESC";
                return GetNews(strSQL);
            }
            public static List<NewsInfo> GetMonthlyFocus(DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT * FROM News" +
                    " WHERE TheDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TheDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " AND IsWeeklyFocus = 0 " +
                    " AND IsMonthlyFocus = 1 " +
                    " ORDER BY TheDate DESC";
                return GetNews(strSQL);
            }
            public static List<NewsInfo> GetTheDayInHistory(DateTime theDate)
            {
                string strSQL = "SELECT * FROM News" +
                    " WHERE DAY(TheDate) = " + theDate.Day +
                    " AND MONTH(TheDate) = " + theDate.Month +
                    " AND IsWeeklyFocus = 0 " +
                    " AND IsMonthlyFocus = 0 " +
                    " ORDER BY TheDate DESC";
                return GetNews(strSQL);
            }
            public static List<NewsInfo> GetNewsLastYear(DateTime dtstart, DateTime dtEnd)
            {
                string strSQL = "SELECT * FROM News" +
                      " WHERE TheDate >= " + DTC.Date.ObtainGoodDT(dtstart.AddYears(-1), true) +
                      " AND TheDate <= " + DTC.Date.ObtainGoodDT(dtEnd.AddYears(-1), true) +
                      " ORDER BY TheDate ASC";
                return GetNews(strSQL);
            }
            private static List<NewsInfo> GetNews(string strSQL)
            {
                List<NewsInfo> data = new List<NewsInfo>();
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetNews(dr));
                
                return data;
            }
            public static List<NewsInfo> GetNews(DateTime startDate, DateTime endDate)
            {
                string strSQL = "SELECT * FROM News" +
                    " WHERE TheDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND TheDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    " ORDER BY TheDate DESC";
                return GetNews(strSQL);
            }
            public static NewsInfo GetNews(long newsItemID)
            {
                NewsInfo info = new NewsInfo();
                DataTable dt = RunExecuteReader("SELECT * FROM News WHERE NewsID = " + newsItemID);

                if (dt.Rows.Count == 1)
                    info = GetNews(dt.Rows[0]);

                return info;
            }
            public static NewsInfo GetNews(DataRow dr)
            {
                NewsInfo info = new NewsInfo();

                info.ID = Convert.ToInt32(dr["NewsID"]);
                info.Date = Convert.ToDateTime(dr["TheDate"]);
                info.Title = Convert.ToString(dr["Title"]);
                info.IsWeeklyFocus = Convert.ToBoolean(Convert.ToInt16(dr["IsWeeklyFocus"]));
                info.IsMonthlyFocus = Convert.ToBoolean(Convert.ToInt16(dr["IsMonthlyFocus"]));
                if (dr["Details"] != DBNull.Value) info.Details = Convert.ToString(dr["Details"]);

                return info;
            }
            public static long AddUpdateNews(NewsInfo news)
            {
                string SQL = "";

                if (news.ID == 0)
                {
                    SQL = "INSERT INTO News " +
                        " (Title,TheDate,Details, IsWeeklyFocus, IsMonthlyFocus" +
                        " ) VALUES (" +
                        "'" + DTC.Control.InputText(news.Title, 50) + "'," +
                        DTC.Date.ObtainGoodDT(news.Date, true) + "," +
                        "'" + DTC.Control.InputText(news.Details, 255) + "'," +
                        Convert.ToInt16(news.IsWeeklyFocus) + "," +
                        Convert.ToInt16(news.IsMonthlyFocus) + 
                        ") SELECT SCOPE_IDENTITY() AS NewsID";
                    news.ID = RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE News SET " +
                        " Title = '" + DTC.Control.InputText(news.Title, 50) + "'," +
                        " TheDate = " + DTC.Date.ObtainGoodDT(news.Date, true) + "," +
                        " Details = '" + DTC.Control.InputText(news.Details, 255) + "'," +
                        " IsWeeklyFocus = " + Convert.ToInt16(news.IsWeeklyFocus) + "," +
                        " IsMonthlyFocus = " + Convert.ToInt16(news.IsMonthlyFocus) + 
                        " WHERE NewsID = " + news.ID;
                    RunNonQuery(SQL);
                }

                return news.ID;
            }
            public static bool DeleteNews(long newsID)
            {
                bool isOK = true;
                string SQL = "";

                if (isOK)
                {
                    SQL = "DELETE News WHERE NewsID = " + newsID;
                    RunNonQuery(SQL);
                }

                return isOK;
            }
        }

        public static DataRow GetSingleDR(DataTable dt, bool isStrictlyOneRow)
        {
            if (dt.Rows.Count > 0)
            {
                if (!isStrictlyOneRow || dt.Rows.Count == 1)
                    return dt.Rows[0];
                else
                    return null;
            }
            else { return null; }
                
        }

        public class Diary
        {
            public static List<DiaryInfo> GetDiaries(DateTime startDate, DateTime endDate, DiaryInfo.NatureEnum nature)
            {
                string natureSQL = "";

                if (nature != DiaryInfo.NatureEnum.NA)
                    natureSQL = " AND DiaryNature = " + (int)nature;

                string SQL = "SELECT * FROM Diary " +
                    " WHERE DiaryDate >= " + DTC.Date.ObtainGoodDT(startDate, true) +
                    " AND DiaryDate <= " + DTC.Date.ObtainGoodDT(endDate, true) +
                    natureSQL +
                    " ORDER BY DiaryDate DESC";
                return GetDiaries(SQL);
            }
            public static List<DiaryInfo> GetDiariesOfProject(long projectID)
            {
                string SQL = "SELECT * FROM Diary " +
                    " WHERE DiaryNature = " + (int)DiaryInfo.NatureEnum.Project +
                    " AND ObjectID = " + projectID +
                    " ORDER BY DiaryDate DESC";
                return GetDiaries(SQL);
            }
            public static DiaryInfo GetDiary(long diaryID)
            {
                DiaryInfo info = new DiaryInfo();
                DataTable dt = RunExecuteReader("SELECT * FROM Diary WHERE DiaryID = " + diaryID);

                if (dt.Rows.Count == 1)
                    info = GetDiary(dt.Rows[0]);

                return info;
            }
            private static List<DiaryInfo> GetDiaries(string strSQL)
            {
                List<DiaryInfo> data = new List<DiaryInfo>();
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetDiary(dr));

                return data;
            }
            public static DiaryInfo GetDiary(DataRow dr)
            {
                DiaryInfo diary = new DiaryInfo();

                diary.ID = Convert.ToInt32(dr["DiaryID"]);
                diary.Title = Convert.ToString(dr["DiaryTitle"]);
                diary.Body = Convert.ToString(dr["DiaryBody"]);
                diary.Date = Convert.ToDateTime(dr["DiaryDate"]);
                diary.Nature = (DiaryInfo.NatureEnum)Convert.ToInt16(dr["DiaryNature"]);
                diary.ObjectID = Convert.ToInt32(dr["ObjectID"]);

                return diary;
            }
            public static long AddUpdateDiary(DiaryInfo diary)
            {
                string SQL = "";

                if (diary.ID == 0)
                {
                    SQL = "INSERT INTO Diary " +
                        " (DiaryTitle, DiaryBody, DiaryDate, DiaryNature, ObjectID" +
                        " ) VALUES (" +
                        "'" + DTC.Control.InputText(diary.Title, 255) + "'," +
                        "'" + DTC.Control.InputTextLight(diary.Body, 9999) + "'," +
                        DTC.Date.ObtainGoodDT(diary.Date, true) + "," +
                        (int)diary.Nature + "," +
                        diary.ObjectID +
                        ") SELECT SCOPE_IDENTITY() AS DiaryID";
                    diary.ID = (int)RunExecuteScalar(SQL);
                }
                else
                {
                    SQL = "UPDATE Diary SET " +
                        " DiaryTitle = '" + DTC.Control.InputText(diary.Title, 255) + "'," +
                        " DiaryBody = '" + DTC.Control.InputTextLight(diary.Body, 9999) + "'," +
                        " DiaryDate = " + DTC.Date.ObtainGoodDT(diary.Date, true) + "," +
                        " DiaryNature = " + Convert.ToInt16(diary.Nature) + "," +
                        " ObjectID = " + diary.ObjectID +
                        " WHERE DiaryID = " + diary.ID;
                    RunNonQuery(SQL);
                }

                return diary.ID;
            }
            public static bool DeleteDiary(long diaryID)
            {
                bool isOK = true;
                string SQL = "";

                if (isOK)
                {
                    SQL = "DELETE Diary WHERE DiaryID = " + diaryID;
                    RunNonQuery(SQL);
                }

                return isOK;
            }
        }

        public class IdeaGroups
        {
            public static IdeaGroupInfo GetIdeaGroup(DataRow dr, bool getTotalIdeas)
            {
                IdeaGroupInfo info = new IdeaGroupInfo();
        
                info.ID = Convert.ToInt32(dr["IdeaGroupID"]);
                info.Title = Convert.ToString(dr["IdeaGroupTitle"]);
                info.CreationDate = Convert.ToDateTime(dr["CreationDate"]);
                if (dr["LastUpdateDate"] != DBNull.Value) info.LastUpdateDate = Convert.ToDateTime(dr["LastUpdateDate"]);
                info.ProjectID = Convert.ToInt32(dr["ProjectID"]);
                info.ProjectGroupID = Convert.ToInt32(dr["ProjectGroupID"]);
                info.ProjectLabelShort= Convert.ToString(dr["ProjectLabelShort"]);
                info.ProjectLabelLong = Convert.ToString(dr["ProjectLabelLong"]);
                info.Details = Convert.ToString(dr["Details"]);
                info.IsFocused = Convert.ToBoolean(dr["IsFocused"]);
                if (getTotalIdeas)
                    info.NumIdeas = Convert.ToInt32(dr["TOTAL"]);

                return info;
            }
            public static IdeaGroupInfo GetIdeaGroup(int ideaGroupID)
            {
                IdeaGroupInfo info = new IdeaGroupInfo();

                string strSQL = "SELECT * " +
                   " FROM IdeaGroups " +
                   " WHERE IdeaGroupID = " + ideaGroupID;

                DataTable dt = RunExecuteReader(strSQL);
                if (dt.Rows.Count == 1)
                    info = GetIdeaGroup(dt.Rows[0], false);

                return info;
            }
            public static List<IdeaGroupInfo> GetIdeaGroups()
            {
                List<IdeaGroupInfo> data = new List<IdeaGroupInfo>();

                string strSQL = "SELECT IdeaGroups.IdeaGroupID, IdeaGroups.IdeaGroupTitle, IdeaGroups.CreationDate, IdeaGroups.LastUpdateDate, " +
                    " IdeaGroups.ProjectGroupID, IdeaGroups.ProjectID, ProjectLabelShort, ProjectLabelLong, IdeaGroups.Details, IdeaGroups.IsFocused, COUNT(Ideas.IdeaID) AS TOTAL " +
                    " FROM IdeaGroups " +
                    " INNER JOIN Ideas ON IdeaGroups.IdeaGroupID = Ideas.IdeaGroupID " +
                    " GROUP BY IdeaGroups.IsFocused, IdeaGroups.LastUpdateDate, IdeaGroups.IdeaGroupID, IdeaGroups.IdeaGroupTitle, " +
                    " IdeaGroups.ProjectLabelShort, IdeaGroups.ProjectLabelLong, " +
                    " IdeaGroups.CreationDate, IdeaGroups.ProjectGroupID, IdeaGroups.ProjectID, IdeaGroups.Details " +
                    " ORDER BY IdeaGroups.IsFocused DESC, IdeaGroups.LastUpdateDate DESC";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetIdeaGroup(dr, true));

                return data;
            }
            public static List<IdeaGroupInfo> GetIdeaGroups(bool isFocusedOnesOnTop, int numIdeaGroups)
            {
                List<IdeaGroupInfo> data = new List<IdeaGroupInfo>();

                string strOrderBY = string.Empty;
                if (isFocusedOnesOnTop)
                    strOrderBY = "IdeaGroups.IsFocused DESC,";

                string strSQL = "SELECT TOP " + numIdeaGroups + " IdeaGroups.IdeaGroupID, IdeaGroups.IdeaGroupTitle, " +
                    " IdeaGroups.CreationDate, IdeaGroups.LastUpdateDate, " +
                    " IdeaGroups.ProjectGroupID, IdeaGroups.ProjectID, ProjectLabelShort, ProjectLabelLong, IdeaGroups.Details, " +
                    " IdeaGroups.IsFocused, COUNT(Ideas.IdeaID) AS TOTAL " +
                    " FROM IdeaGroups " +
                    " LEFT OUTER JOIN Ideas ON IdeaGroups.IdeaGroupID = Ideas.IdeaGroupID " +
                    " GROUP BY IdeaGroups.IsFocused, IdeaGroups.LastUpdateDate, IdeaGroups.IdeaGroupID, ProjectLabelShort, ProjectLabelLong, IdeaGroups.IdeaGroupTitle, " +
                    " IdeaGroups.CreationDate, IdeaGroups.ProjectGroupID, IdeaGroups.ProjectID, IdeaGroups.Details " +
                    " ORDER BY " + strOrderBY + " IdeaGroups.LastUpdateDate DESC";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetIdeaGroup(dr, true));

                return data;
            }
            public static void TempAssignProjectLabels()
            {
                List<IdeaGroupInfo> ideaGroupsAll = GetIdeaGroups();

                string strSQL = "SELECT IdeaGroups.IdeaGroupID, IdeaGroups.IdeaGroupTitle, IdeaGroups.CreationDate, IdeaGroups.LastUpdateDate, IdeaGroups.ProjectGroupID, " +
                    " IdeaGroups.ProjectID, IdeaGroups.Details, IdeaGroups.IsFocused, COUNT(Ideas.IdeaID) AS TOTAL, Projects.ProjectName, Projects.ProjectCode, " +
                    " ProjectGroups.ProjectGroupCode, ProjectGroups.ProjectGroupName " +
                    " FROM IdeaGroups " +
                    " INNER JOIN Ideas ON IdeaGroups.IdeaGroupID = Ideas.IdeaGroupID " +
                    " LEFT OUTER JOIN Projects ON IdeaGroups.ProjectID = Projects.ProjectID " +
                    " LEFT OUTER JOIN ProjectGroups ON IdeaGroups.ProjectGroupID = ProjectGroups.ProjectGroupID " +
                    " GROUP BY IdeaGroups.IsFocused, IdeaGroups.LastUpdateDate, IdeaGroups.IdeaGroupID, IdeaGroups.IdeaGroupTitle, IdeaGroups.CreationDate, " +
                    " IdeaGroups.ProjectGroupID, IdeaGroups.ProjectID, IdeaGroups.Details, Projects.ProjectName, Projects.ProjectCode, ProjectGroups.ProjectGroupCode, " +
                    " ProjectGroups.ProjectGroupName ORDER BY IdeaGroups.IsFocused DESC, IdeaGroups.LastUpdateDate DESC";

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    long ideaGroupID = Convert.ToInt32(dr["IdeaGroupID"]);
                    IdeaGroupInfo iG = ideaGroupsAll.Find(i=>i.ID == ideaGroupID);

                    if(iG != null)
                    {
                        iG.ProjectLabelShort = "";
                    }
                }
                
            }

            public static Dictionary<int, IdeaGroupInfo> GetIdeaGroups(int projectGroupID, int projectID, DateTime startDate, DateTime endDate)
            {
                Dictionary<int, IdeaGroupInfo> dict = new Dictionary<int, IdeaGroupInfo>();

                string filterSQL = " WHERE IdeaGroups.IdeaGroupID > 0";

                if (projectID == 0 && projectGroupID == 0) filterSQL += "";
                else if (projectID > 0) filterSQL += " AND IdeaGroups.ProjectID=" + projectID;
                else if (projectID == 0) filterSQL += " AND IdeaGroups.ProjectGroupID=" + projectGroupID;

                if (startDate > DateTime.MinValue)
                    filterSQL += " AND IdeaGroups.CreationDate >= " + DTC.Date.ObtainGoodDT(startDate, true);
                if (endDate < DateTime.MaxValue)
                    filterSQL += " AND IdeaGroups.CreationDate <= " + DTC.Date.ObtainGoodDT(endDate, true);

                string SQL = "SELECT IdeaGroups.*, " +
                    " COUNT(Ideas.IdeaID) AS TOTAL" +
                    " FROM IdeaGroups" +
                    " LEFT OUTER JOIN Ideas ON IdeaGroups.IdeaGroupID = Ideas.IdeaGroupID" +
                    filterSQL +
                    " GROUP BY IdeaGroups.IdeaGroupID, IdeaGroups.IdeaGroupTitle, IdeaGroups.CreationDate," +
                    " IdeaGroups.LastUpdateDate, IdeaGroups.ProjectGroupID, IdeaGroups.ProjectID," +
                    " IdeaGroups.ProjectLabelShort, IdeaGroups.ProjectLabelLong, " +
                    " IdeaGroups.XIconID, IdeaGroups.Details, " +
                    " IdeaGroups.LocationX, IdeaGroups.LocationY, IdeaGroups.IsFocused," +
                    " IdeaGroups.MinLocationX, IdeaGroups.MinLocationY, IdeaGroups.IsMinimized," +
                    " IdeaGroups.ZOrder, IdeaGroups.DesktopID " +
                    " ORDER BY IdeaGroups.LastUpdateDate DESC";

                return GetIdeaGroups(SQL);
            }
            public static Dictionary<int, IdeaGroupInfo> GetIdeaGroups(int projectGroupID, int projectID)
            {
                DateTime startDate;
                DateTime endDate;

                startDate = DateTime.Today.AddYears(-100);
                endDate = DateTime.Today.AddYears(100);

                return GetIdeaGroups(projectGroupID, projectID, startDate, endDate);
            }
            public static Dictionary<int, IdeaGroupInfo> GetFocusedIdeaGroups()
            {
                Dictionary<int, IdeaGroupInfo> dict = new Dictionary<int, IdeaGroupInfo>();

                string SQL = "SELECT IdeaGroups.*, " +
                    " COUNT(Ideas.IdeaID) AS TOTAL" +
                    " FROM IdeaGroups" +
                    " LEFT OUTER JOIN Ideas ON IdeaGroups.IdeaGroupID = Ideas.IdeaGroupID" +
                    " WHERE IdeaGroups.IsFocused = 1 " +
                    " GROUP BY IdeaGroups.IdeaGroupID, IdeaGroups.IdeaGroupTitle, IdeaGroups.CreationDate," +
                    " IdeaGroups.LastUpdateDate, IdeaGroups.ProjectGroupID, IdeaGroups.ProjectID," +
                    " IdeaGroups.ProjectLabelShort, IdeaGroups.ProjectLabelLong, " +
                    " IdeaGroups.XIconID, IdeaGroups.Details, " +
                    " IdeaGroups.LocationX, IdeaGroups.LocationY, IdeaGroups.IsFocused, " +
                    " IdeaGroups.MinLocationX, IdeaGroups.MinLocationY, IdeaGroups.IsMinimized, " +
                    " IdeaGroups.ZOrder,IdeaGroups.DesktopID " +
                    " ORDER BY IdeaGroups.LastUpdateDate DESC";

                return GetIdeaGroups(SQL);
            }
            public static Dictionary<int, IdeaGroupInfo> GetIdeaGroups(string strSQL)
            {
                Dictionary<int, IdeaGroupInfo> dict = new Dictionary<int, IdeaGroupInfo>();

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    dict.Add(Convert.ToInt32(dr["IdeaGroupID"]), GetIdeaGroup(dr, true));
                
                return dict;
            }
            public static void AddUpdateIdeaGroup(IdeaGroupInfo ideaGroup)
            {
                string strSQL = "";

                if (ideaGroup.ID > 0)
                {
                    strSQL = "UPDATE IdeaGroups " +
                        " SET" +
                        " IdeaGroupTitle = '" + DTC.Control.InputText(ideaGroup.Title, 99) + "'," +
                        " CreationDate = " + DTC.Date.ObtainGoodDT(ideaGroup.CreationDate, true) + "," +
                        " LastUpdateDate = " + DTC.Date.ObtainGoodDT(ideaGroup.LastUpdateDate, true) + "," +
                        " ProjectGroupID = " + ideaGroup.ProjectGroupID + "," +
                        " ProjectLabelShort = '" + DTC.Control.InputText(ideaGroup.ProjectLabelShort, 50) + "'," +
                        " ProjectLabelLong = '" + DTC.Control.InputText(ideaGroup.ProjectLabelLong, 50) + "'," +
                        " ProjectID = " + ideaGroup.ProjectID + "," +
                        " Details = '" + DTC.Control.InputText(ideaGroup.Details, 255) + "'," +
                        " IsFocused = " + Convert.ToInt16(ideaGroup.IsFocused) + 
                        " WHERE IdeaGroupID = " + ideaGroup.ID;
                }
                else
                {
                    strSQL = "INSERT INTO IdeaGroups " +
                        " (IdeaGroupTitle, CreationDate, LastUpdateDate, ProjectGroupID, ProjectID, " +
                        " ProjectLabelShort, ProjectLabelLong, " +
                        " Details, IsFocused " +
                        " )" +
                        " VALUES (" +
                        "'" + DTC.Control.InputText(ideaGroup.Title, 99) + "'," +
                        DTC.Date.ObtainGoodDT(ideaGroup.CreationDate, true) + "," +
                        DTC.Date.ObtainGoodDT(ideaGroup.LastUpdateDate, true) + "," +
                        ideaGroup.ProjectGroupID + "," +
                        ideaGroup.ProjectID + "," +
                        "'" + DTC.Control.InputText(ideaGroup.ProjectLabelShort, 50) + "'," +
                        "'" + DTC.Control.InputText(ideaGroup.ProjectLabelLong, 50) + "'," +
                        "'" + DTC.Control.InputText(ideaGroup.Details, 255) + "'," +
                        Convert.ToInt16(ideaGroup.IsFocused) + 
                        ")";
                }

                RunNonQuery(strSQL);
            }
            public static void DeleteIdeaGroup(long ideaGroupID)
            {
                string strSQL = "DELETE FROM Ideas " +
                    " WHERE IdeaGroupID = " + ideaGroupID;
                RunNonQuery(strSQL);

                strSQL = "DELETE FROM IdeaGroups " +
                    " WHERE IdeaGroupID = " + ideaGroupID;
                RunNonQuery(strSQL);
            }
        }

        public class Ideas
        {
            public static void AddUpdateIdea(IdeaInfo idea)
            {
                string strSQL = "";

                if (idea.ID > 0)
                {
                    strSQL = "UPDATE Ideas " +
                        " SET" +
                        " IdeaTitle = '" + DTC.Control.InputText(idea.Title, 255) + "'," +
                        " CreationDate = " + DTC.Date.ObtainGoodDT(idea.CreationDate, true) + "," +
                        " IdeaGroupID = " + idea.IdeaGroupID + "," +
                        " IdeaOrder = " + idea.Order + "," +
                        " Impact = " + (int)idea.Impact + "," +
                        " Status = " + (int)idea.Status + "," +
                        " Details = '" + DTC.Control.InputTextLight(idea.Details, 500) + "'," +
                        " IsFocused = " + Convert.ToInt16(idea.IsFocused) + "," +
                        " IsActionable = " + Convert.ToInt16(idea.IsActionable) + "," +
                        " ActionDueDate = " + DTC.Date.ObtainGoodDT(idea.ActionDueDate, true) +
                        " WHERE IdeaID = " + idea.ID;
                }
                else
                {
                    strSQL = "INSERT INTO Ideas " +
                        " (IdeaTitle, CreationDate,IdeaGroupID ,IdeaOrder, Impact, Status, Details," +
                        " IsFocused, IsActionable, ActionDueDate)" +
                        " VALUES (" +
                        "'" + DTC.Control.InputText(idea.Title, 255) + "'," +
                        DTC.Date.ObtainGoodDT(idea.CreationDate, true) + "," +
                        idea.IdeaGroupID + "," +
                        idea.Order + "," +
                        (int)idea.Impact + "," +
                        (int)idea.Status + "," +
                        "'" + DTC.Control.InputTextLight(idea.Details, 500) + "'," +
                        Convert.ToInt16(idea.IsFocused) + "," +
                        Convert.ToInt16(idea.IsActionable) + "," +
                        DTC.Date.ObtainGoodDT(idea.ActionDueDate, true) +
                        ")";
                }

                RunNonQuery(strSQL);

                strSQL = "UPDATE IdeaGroups " +
                    " SET LastUpdateDate = " + DTC.Date.ObtainGoodDT(DateTime.Today, true) +
                    " WHERE IdeaGroupID = " + idea.IdeaGroupID;
                RunNonQuery(strSQL);
            }
            public static IdeaInfo GetIdea(DataRow dr)
            {
                IdeaInfo info = new IdeaInfo();

                info.ID = Convert.ToInt32(dr["IdeaID"]);
                info.Title = Convert.ToString(dr["IdeaTitle"]);
                info.CreationDate = Convert.ToDateTime(dr["CreationDate"]);
                info.Order = Convert.ToInt32(dr["IdeaOrder"]);
                info.IdeaGroupID = Convert.ToInt32(dr["IdeaGroupID"]);
                info.Details = Convert.ToString(dr["Details"]);
                info.Impact = (DTC.SizeEnum)Convert.ToInt32(dr["Impact"]);
                info.Status = (DTC.StatusEnum)Convert.ToInt32(dr["Status"]);
                info.InnovativePoint = Convert.ToInt32(dr["InnovativePoint"]);
                info.IsFocused = Convert.ToBoolean(dr["IsFocused"]);
                info.IsActionable = Convert.ToBoolean(dr["IsActionable"]);
                if (dr["ActionDueDate"] != DBNull.Value)
                    info.ActionDueDate = Convert.ToDateTime(dr["ActionDueDate"]);

                if (dr.Table.Columns.Contains("ProjectName") && dr.Table.Columns.Contains("ProjectGroupName"))
                    info.ProjectNameLazy = Convert.ToString(dr["ProjectGroupName"]) + " | " + Convert.ToString(dr["ProjectName"]);
                
                if (dr.Table.Columns.Contains("ProjectImgName"))
                    info.ImageNameLazy = Convert.ToString(dr["ProjectImgName"]);

                return info;
            }
            public static IdeaInfo GetIdea(int ideaID)
            {
                IdeaInfo info = new IdeaInfo();

                string strSQL = "SELECT * " +
                   " FROM Ideas " +
                   " WHERE IdeaID = " + ideaID;
                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    info = GetIdea(dr);
                
                return info;
            }
            public static List<IdeaInfo> GetIdeas(int ideaGroupID)
            {
                string strSQL = "SELECT * FROM Ideas " +
                    " WHERE IdeaGroupID = " + ideaGroupID +
                    " ORDER BY IdeaOrder ASC";
                return GetIdeas(strSQL);
            }
            public static List<IdeaInfo> GetIdeas(ProjectInfo project, DTC.SizeEnum minImpact)
            {
                string strSQL = "SELECT * " +
                    " FROM Ideas" +
                    " INNER JOIN IdeaGroups ON Ideas.IdeaGroupID = IdeaGroups.IdeaGroupID" +
                    " WHERE Ideas.Impact >= " + (int)minImpact +
                    " AND IdeaGroups.ProjectID = " + project.ID +
                    " ORDER BY Impact DESC,IdeaOrder ASC";
                return GetIdeas(strSQL);
            }
            public static List<IdeaInfo> GetIdeas(DateTime d1, DateTime d2, DTC.SizeEnum minImpact)
            {
                string strSQL= "SELECT Ideas.*, Projects.ProjectName, Projects.ProjectCode, Projects.ProjectImgName, ProjectGroups.ProjectGroupName, ProjectGroups.ProjectGroupCode " +
                    " FROM Ideas " +
                    " INNER JOIN IdeaGroups ON Ideas.IdeaGroupID = IdeaGroups.IdeaGroupID " +
                    " INNER JOIN Projects ON IdeaGroups.ProjectID = Projects.ProjectID " +
                    " INNER JOIN ProjectGroups ON IdeaGroups.ProjectGroupID = ProjectGroups.ProjectGroupID " +
                    " WHERE Ideas.CreationDate >= " + DTC.Date.ObtainGoodDT(d1, true) +
                    " AND Ideas.CreationDate <= " + DTC.Date.ObtainGoodDT(d2, true) +
                    " AND Ideas.Impact >= " + (int)minImpact +
                    " ORDER BY Ideas.Impact DESC, Ideas.IdeaOrder";
                return GetIdeas(strSQL);
            }
            public static List<IdeaInfo> GetActionableIdeas(ProjectInfo project)
            {
                string strProject = "";
                if (project.ID > 0)
                    strProject = " AND IdeaGroups.ProjectID = " + project.ID;
                string strSQL = "SELECT Ideas.* " +
                    " FROM Ideas, IdeaGroups " +
                    " WHERE Ideas.IdeaGroupID = IdeaGroups.IdeaGroupID " +
                    " AND Ideas.IsActionable = 1 " +
                    " AND Ideas.Status = " + (int)DTC.StatusEnum.Running +
                    strProject +
                    " ORDER BY Ideas.ActionDueDate ASC";
                return GetIdeas(strSQL);
            }
            public static List<IdeaInfo> GetIdeas(string strSQL)
            {
                List<IdeaInfo> data = new List<IdeaInfo>();

                DataTable dt = RunExecuteReader(strSQL);
                foreach (DataRow dr in dt.Rows)
                    data.Add(GetIdea(dr));
                
                return data;
            }
            public static void ChangeOrder(int id, int order)
            {
                string strSQL = "UPDATE Ideas SET" +
                    " IdeaOrder = " + order +
                    " WHERE IdeaID = " + id;
                RunNonQuery(strSQL);
            }
            public static void DeleteIdea(int ideaID)
            {
                string strSQL = "DELETE FROM Ideas " +
                    " WHERE IdeaID = " + ideaID;
                RunNonQuery(strSQL);
            }
        }
    }
}