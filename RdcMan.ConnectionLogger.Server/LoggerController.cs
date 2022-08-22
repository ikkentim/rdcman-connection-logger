using Microsoft.AspNetCore.Mvc;

namespace RdcMan.ConnectionLogger.Server;

[Route("api/[controller]")]
[ApiController]
public class LoggerController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => new JsonResult(LoggerStorage.GetEntries().Select(delegate(LoggerEntry x)
    {
        // We're using a crappy json deserializer on the client end so we need to convert date to unix timestamp.
        var dto = new DateTimeOffset(x.Date.ToUniversalTime());
        return new LoggerEntryLngDate
        { 
            Action = x.Action, 
            Date = dto.ToUnixTimeSeconds(), 
            RemoteAddress = x.RemoteAddress, 
            RemoteName = x.RemoteName, 
            UserName = x.UserName
        };
    }));

    [HttpPost]
    public IActionResult Add([FromBody]LoggerEntry entry)
    {
        LoggerStorage.AddEntry(entry);
        return Ok();
    }
}