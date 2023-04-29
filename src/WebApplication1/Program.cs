using Service.Abstractions;
using Service.Sqs;
using Service.Sqs.Extensions;
using WebApplication1.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISubscribeSqs<NewRecord>, NewRecordSqsHandler>();
builder.Host.UseEffectSqs(AppNameType.ServiceA, (x, sp) => x with
{
    IsGreenCircuitBreakAsync = () => Task.FromResult(true)
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
