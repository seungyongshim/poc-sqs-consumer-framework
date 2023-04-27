using WebApplication1;
using WebApplication1.Controllers;
using WebApplication1.Dto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISubscribeSqs<HelloDto>, HelloDtoHandler>();
builder.Host.UseSqs(x =>
{
    x.SqsUrls.Add(new SqsUrlsContext
    {
        Url = "http://localhost:4576/queue/test",
        Parallelism = 10
    });
});


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
