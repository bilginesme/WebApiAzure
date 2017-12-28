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
            List<BlockInfo> blocks = DB.GetBlocks(projectID);

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

        [HttpDelete]
        [Route("api/Blocks/{blockID}")]
        public string Delete(long blockID)
        {
            DB.Blocks.DeleteBlock(blockID);
            return "ok";
        }
    }
}