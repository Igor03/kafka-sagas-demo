 
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Producer.Controllers;

[ApiController]
[Route("[controller]")]
public class KafkaController : ControllerBase
{
    private readonly ITopicProducer<string, OrderRequest> _producer;

    public KafkaController(ITopicProducer<string, OrderRequest> producer)
    {
        _producer = producer;
    }

    [HttpPost(Name = "Produce")]
    public async Task<ActionResult> Post([FromBody] OrderRequest orderRequest)
    {
        var key = Guid.NewGuid();

        orderRequest.OrderId = key;
        
        await _producer
            .Produce(key.ToString(), orderRequest)
            .ConfigureAwait(false);

        return Accepted(orderRequest);
    }
}