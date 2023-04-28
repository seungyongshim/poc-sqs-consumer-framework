using WebApplication1;
using WebApplication1.Controllers;
using WebApplication1.Dto;
using WebApplication1.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddTransient<ISubscribeSqs<HelloDto>, HelloDtoHandler>();
builder.Host.UseEffectSqs(AppServiceType.ServiceA, (x, sp) => x);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
