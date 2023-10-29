using CustomerValidationEngine.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomKafka(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();