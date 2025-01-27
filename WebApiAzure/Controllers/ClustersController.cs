using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class ClustersController : ApiController
    {
        [Route("api/Clusters/{projectID}/{computeAllParameters}")]
        public IEnumerable<ClusterInfo> Get(int projectID, bool computeAllParameters)
        {
            List<ClusterInfo> clusters = DB.Clusters.GetClustersOfProject(projectID, true);

            return clusters;
        }

        [Route("api/Clusters/{subProjectID}/{computeAllParameters}/{removeThisParameter}")]
        public IEnumerable<ClusterInfo> Get(int subProjectID, bool computeAllParameters, int removeThisParameter)
        {
            List<ClusterInfo> clusters = DB.Clusters.GetClustersOfSubProject(subProjectID, true);

            return clusters;
        }

        [HttpGet]
        [Route("api/Clusters/{clusterID}")]
        public ClusterInfo Get(long clusterID)
        {
            return DB.Clusters.GetCluster(clusterID);
        }
         
        [HttpPost]
        [Route("api/Clusters/")]
        public void Post([FromBody] ClusterInfo cluster)
        {
            DB.Clusters.AddUpdateCluster(cluster);
        }

        [HttpPut]
        [Route("api/Clusters/{clusterID}")]
        public void Put(long clusterID, [FromBody] ClusterInfo cluster)
        {
            DB.Clusters.AddUpdateCluster(cluster);
        }

        [HttpDelete]
        [Route("api/Clusters/{clusterID}")]
        public string Delete(long clusterID)
        {
            DB.Clusters.DeleteCluster(clusterID);
            return "ok";
        }
    }
}
