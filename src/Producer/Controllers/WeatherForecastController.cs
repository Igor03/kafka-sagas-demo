 
using Microsoft.AspNetCore.Mvc;

namespace Producer.Controllers;

[ApiController]
[Route("[controller]")]
public class KafkaController : ControllerBase
{
    public KafkaController()
    {
    }

    [HttpPost(Name = "Request")]
    public IActionResult Post([FromBody] SourceSystemRequest request)
    {
        return Ok("3454");
    }
}