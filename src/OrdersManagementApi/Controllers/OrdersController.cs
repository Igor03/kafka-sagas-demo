using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrdersManagementApi.Contracts;

namespace OrdersManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ITopicProducer<string, OrderRequest> _producer;

    public OrdersController(ITopicProducer<string, OrderRequest> producer)
    {
        _producer = producer;
    }

    [HttpPost(Name = "Place")]
    public async Task<ActionResult> Post([FromBody] OrderRequest orderRequest)
    {
        var key = Guid.NewGuid();

        orderRequest.CorrelationId = key;
        
        await _producer
            .Produce(key.ToString(), orderRequest)
            .ConfigureAwait(false);

        return Accepted(orderRequest);
    }
}