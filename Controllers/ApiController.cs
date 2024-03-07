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
            string tmpStr = "|API|Test Patch Server 2";
            logger.LogInformation(tmpStr);
            return Ok(tmpStr);
        }

        [HttpGet]
        public ActionResult<string> CharacterTable()
        {
            return Ok($"{JsonConvert.SerializeObject(fireBase.DataClient.Characters)}");
        }
    }
}