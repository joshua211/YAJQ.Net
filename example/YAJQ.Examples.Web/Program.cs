using Microsoft.AspNetCore.ResponseCompression;
using YAJQ.DependencyInjection;
using YAJQ.Examples.Web;
using YAJQ.Examples.Web.Hubs;
using YAJQ.Hosted;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] {"application/octet-stream"});
});
builder.Services.AddYAJQ();
builder.Services.AddHostedJobHandler();
builder.Services.AddTransient<JobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<JobHub>("/jobs");
app.MapFallbackToPage("/_Host");

app.Run();