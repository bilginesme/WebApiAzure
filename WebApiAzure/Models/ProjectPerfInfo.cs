using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class ProjectPerfInfo
    {
        int projectID;
        float numTodos;
        float numCompleted;
        float size;
        float completedSize;

        public ProjectPerfInfo()
        {
            projectID = 0;
            numTodos = 0;
            numCompleted = 0;
            size = 0;
            completedSize = 0;
        }
        public float GetPerformance()
        {
            float performance = 0;
            if (size > 0)
                performance = 100 * (completedSize / size);
            else
                performance = 0;

            return performance;
        }

        public int ProjectID { get { return projectID; } set { projectID = value; }}
        public float NumTodos { get { return numTodos; } set { numTodos = value; }}
        public float NumCompleted { get { return numCompleted; } set { numCompleted = value; }}
        public float Size { get { return size; } set { size = value; }}
        public float CompletedSize { get { return completedSize; } set { completedSize = value; }
        }
    }
}