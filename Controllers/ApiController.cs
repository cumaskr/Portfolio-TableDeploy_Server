using Microsoft.AspNetCore.Mvc;

namespace APIServer
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger logger;

        public ApiController(ILogger<ApiController> lg) 
        {
            logger = lg;
        }

        [HttpGet]
        public ActionResult<string> Test()
        {
            logger.LogInformation("|API|Test");
            return Ok("Test");
        }

        [HttpGet]
        public ActionResult<string> GetJson(string jsonName)
        {
            return Ok($"GetJson/{jsonName}");
        }
    }
}