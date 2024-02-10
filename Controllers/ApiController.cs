using Microsoft.AspNetCore.Mvc;

namespace APIServer
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> GetJson(string jsonName)
        {
            return Ok($"GetJson/{jsonName}");
        }

        [HttpGet]
        public ActionResult<string> Test()
        {
            return Ok("Test");
        }
    }
}