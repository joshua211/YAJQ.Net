using Fastjob.Core;
using Fastjob.DependencyInjection;
using Fastjob.Example;
using Fastjob.Hosted;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFastjob(options => options.HandlerTimeout = 1500);
builder.Services.AddHostedJobHandler();
builder.Services.AddSingleton<IRequestHandler, RequestHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();