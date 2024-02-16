using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace APIServer
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly FireBase fireBase;

        public ApiController(ILogger<ApiController> lg, FireBase fb) 
        {
            logger = lg;
            fireBase = fb;
        }

        [HttpGet]
        public ActionResult<string> Test()
        {
            logger.LogInformation("|API|Test");
            return Ok("Test");
        }

        [HttpGet]
        public ActionResult<string> GetTable(string tableName)
        {
            if (tableName == "Character")
            {
                if (null != fireBase.DataClient.Characters)
                {
                    return Ok($"{JsonConvert.SerializeObject(fireBase.DataClient.Characters)}");
                }
            }
            else if (tableName == "Dungeon") 
            {
                return Ok($"{JsonConvert.SerializeObject(fireBase.DataClient.Dungeons)}");
            }

            return Ok();
        }
    }
}