using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RdcMan.ConnectionLogger.Server
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoggerController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(LoggerStorage.GetEntries());
        }

        [HttpPost]
        public IActionResult Add([FromBody]LoggerEntry entry)
        {
            LoggerStorage.AddEntry(entry);
            return Ok();
        }
    }
}
