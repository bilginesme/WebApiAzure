using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class SegmentPerfInfo
    {
        int blockID;
        int segmentID;
        float numTodos;
        float numCompleted;
        float size;
        DTC.StatusEnum status;

        public SegmentPerfInfo()
        {
            blockID = 0;
            segmentID = 0;
            numTodos = 0;
            numCompleted = 0;
            size = 0;
            status = 0;
        }

        public float GetCompletedSize()
        {
            float completedSize = 0;

            if (status == DTC.StatusEnum.Running)
            {
                if (numTodos > 0)
                    completedSize += size * (numCompleted / numTodos);
                else
                    completedSize += 0;
            }
            else
            {
                completedSize += size;
            }

            return completedSize;
        }
        public float GetPerformance()
        {
            float performance = 0;

            if (numTodos > 0)
                performance = 100 * (numCompleted / numTodos);
            else
                performance = 0;

            return performance;
        }

        public int SegmentID { get { return segmentID; } set { segmentID = value; }}
        public int BlockID { get { return blockID; } set { blockID = value; }}
        public float NumTodos { get { return numTodos; } set { numTodos = value; }}
        public float NumCompleted { get { return numCompleted; } set { numCompleted = value; }}
        public float Size { get { return size; } set { size = value; }}
        public DTC.StatusEnum Status { get { return status; } set { status = value; }
        }
    }
}