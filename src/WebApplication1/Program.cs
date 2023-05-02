using Service.Abstractions;
using Service.Echo;
using Service.Sqs.Abstractions;
using Service.Sqs.Extensions;
using WebApplication1.Controllers;
using WebApplication1.SqsHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISubscribeSqs<NewRecord>, AbstractNewRecordHandler>();
builder.Services.AddTransient<ISubscribeSqs<NewRecord2>, AbstractNewRecordHandler>();

builder.Host.UseEchoService();
builder.Host.UseEffectSqs(AppNameType.App1, (x, sp) => x with
{
    IsGreenCircuitBreakAsync = () => Task.FromResult(true)
});
builder.Host.UseEffectSqs(AppNameType.App2, (x, sp) => x with
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
