 
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Producer.Controllers;

[ApiController]
[Route("[controller]")]
public class KafkaController : ControllerBase
{
    private readonly ITopicProducer<string, SourceSystemRequest> _producer;

    public KafkaController(ITopicProducer<string, SourceSystemRequest> producer)
    {
        _producer = producer;
    }

    [HttpPost(Name = "Produce")]
    public async Task<ActionResult> Post([FromBody] SourceSystemRequest message)
    {
        var key = Guid.NewGuid().ToString();

        await _producer
            .Produce(key, message)
            .ConfigureAwait(false);

        return Created(key, message);
    }
}