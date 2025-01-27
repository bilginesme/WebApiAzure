using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class BlocksController : ApiController
    {
        [Route("api/Blocks/{projectID}/{isOnlyRunning}")]
        public IEnumerable<BlockInfo> Get(int projectID, bool isOnlyRunning)
        {
            List<BlockInfo> blocks = new List<BlockInfo>();

            if (isOnlyRunning)
                blocks = DB.GetBlocksOnlyRunning(projectID);
            else
                blocks = DB.GetBlocks(projectID);

            return blocks;
        }

        [Route("api/Blocks/{projectID}/{clusterID}/{isOnlyRunning}")]
        public IEnumerable<BlockInfo> Get(int projectID, long clusterID, bool isOnlyRunning)
        {
            List<BlockInfo> blocks = DB.Blocks.GetBlocksOfCluster(clusterID);

            return blocks;
        }

        [Route("api/Blocks/{projectID}/{param1}/{param2}/{param3}")]
        public IEnumerable<BlockInfo> Get(int projectID, string param1, string param2, string param3)
        {
            List<BlockInfo> blocks = new List<BlockInfo>();

            if (param1 == "LATEST")
            {
                int numBlocks = Convert.ToInt16(param2);

                blocks = DB.Blocks.GetBlocksCompleted(projectID, numBlocks);
            }
            else if (param1 == "FROM_STRING_SPLIT")
            {
                blocks = DB.Blocks.GetBlocksFromStringSplit(param2);
            }

            return blocks;
        }

        [HttpGet]
        [Route("api/Blocks/{blockID}")]
        public BlockInfo Get(long blockID)
        {
            return DB.GetBlock(blockID);
        }

        [HttpPost]
        [Route("api/Blocks/")]
        public void Post([FromBody] BlockInfo block)
        {
            DB.Blocks.AddUpdateBlock(block);
        }

        [HttpPut]
        [Route("api/Blocks/{blockID}")]
        public void Put(long blockID, [FromBody] BlockInfo block)
        {
            DB.Blocks.AddUpdateBlock(block);
        }

        [HttpPut]
        [Route("api/Blocks/{param}/{blockID}")]
        public bool Put(string param, long blockID, [FromBody] BlockInfo block)
        {
            if(param == "COMPLETE")
            {
                return DB.Blocks.CompleteTheBlock(blockID);
            }
            else
            {
                return false;
            }
        }

        [HttpDelete]
        [Route("api/Blocks/{blockID}")]
        public string Delete(long blockID)
        {
            DB.Blocks.DeleteBlock(blockID);
            return "ok";
        }
    }
}