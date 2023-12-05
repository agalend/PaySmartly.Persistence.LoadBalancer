using Microsoft.AspNetCore.Server.Kestrel.Core;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PaySmartly.Persistence.LoadBalancer.ReverseProxy;
using Yarp.ReverseProxy.Configuration;

// TODO: set service name somewhere!!!
string ServiceName = "Persistance.LoadBalancer Service";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
   // TODO: options.ListenAnyIP if we have env var
   options.ListenLocalhost(9087, listenOptions =>
   {
      listenOptions.Protocols = HttpProtocols.Http2;
   });
});

AddOpenTelemetryLogging(builder);

builder.Services.AddTransient<IProxyConfigProvider, YarpProxyConfigProvider>();
builder.Services.AddReverseProxy();

AddOpenTelemetryService(builder);

var app = builder.Build();


app.MapReverseProxy();
app.Run();

void AddOpenTelemetryLogging(WebApplicationBuilder builder)
{
   builder.Logging.AddOpenTelemetry(options =>
   {
      ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault().AddService(ServiceName);

      options.SetResourceBuilder(resourceBuilder).AddConsoleExporter();
   });
}

void AddOpenTelemetryService(WebApplicationBuilder builder)
{
   OpenTelemetryBuilder openTelemetryBuilder = builder.Services.AddOpenTelemetry();

   openTelemetryBuilder = openTelemetryBuilder.ConfigureResource(resource => resource.AddService(ServiceName));

   openTelemetryBuilder = openTelemetryBuilder.WithTracing(tracing =>
   {
      tracing.AddAspNetCoreInstrumentation().AddConsoleExporter();
   });
   openTelemetryBuilder = openTelemetryBuilder.WithMetrics(metrics =>
   {
      metrics.AddAspNetCoreInstrumentation().AddConsoleExporter();
   });
}