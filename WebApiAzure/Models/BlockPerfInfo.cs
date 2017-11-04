using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiAzure.Models
{
    public class BlockPerfInfo
    {
        float size;
        float completedSize;
        float numTodos;
        float numCompleted;

        public BlockPerfInfo()
        {
            size = 0;
            completedSize = 0;
            numTodos = 0;
            numCompleted = 0;
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

        public float Size
        {
            get { return size; }
            set { size = value; }
        }
        public float CompletedSize
        {
            get { return completedSize; }
            set { completedSize = value; }
        }

        public float NumCompleted
        {
            get { return numCompleted; }
            set { numCompleted = value; }
        }
        public float NumTodos
        {
            get { return numTodos; }
            set { numTodos = value; }
        }
    }
}