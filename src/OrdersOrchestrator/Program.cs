using OrdersOrchestrator.Extensions;
using OrdersOrchestrator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomKafka(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddTransient<IApiService, ApiService>();
// builder.Services.AddRequestHandlers();

var app = builder.Build();

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();