using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace OrdersManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly ITopicProducer<string, OrderRequestEvent> _producer;

    public OrdersController(ITopicProducer<string, OrderRequestEvent> producer)
    {
        _producer = producer;
    }

    [HttpPost(Name = "Place")]
    public async Task<ActionResult> Post([FromBody] OrderRequestEvent orderRequest)
    {
        var key = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        
        await _producer
            .Produce(key.ToString(), orderRequest)
            .ConfigureAwait(false);

        return Accepted(new
        {
            CorrelationId = correlationId,
            Key = key
        });
    }
}