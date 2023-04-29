using Service.Abstractions;
using Service.Sqs;
using Service.Sqs.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISubscribeSqs<string>, StringSqsHandler>();
builder.Host.UseEffectSqs(AppServiceType.ServiceA, (x, sp) => x with
{
    IsGreenCircuitBreakAsync = async _ => await Task.FromResult(true)
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
