using Microsoft.AspNetCore.Server.Kestrel.Core;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PaySmartly.Persistence.LoadBalancer;
using PaySmartly.Persistence.LoadBalancer.Env;
using PaySmartly.Persistence.LoadBalancer.ReverseProxy;
using Yarp.ReverseProxy.Configuration;

string ServiceName = "Persistance.LoadBalancer Service";

var builder = WebApplication.CreateSlimBuilder(args);

ConfigureKestrel(builder);
AddOpenTelemetryLogging(builder);

builder.Services.AddSingleton<IEnvProvider, EnvProvider>();
builder.Services.AddTransient<IProxyConfigProvider, YarpProxyConfigProvider>();
builder.Services.AddReverseProxy();
AddOpenTelemetryService(builder);

var app = builder.Build();
app.MapReverseProxy();
app.Run();

void ConfigureKestrel(WebApplicationBuilder builder)
{
   builder.WebHost.ConfigureKestrel(options =>
   {
      KestrelConfig? config = EnvProvider.Instance.GetKestrelConfig();

      if (config?.ListenAnyIP == true)
      {
         options.ListenAnyIP(config.Port, listenOptions =>
         {
            listenOptions.Protocols = HttpProtocols.Http2;
         });
      }
      else
      {
         int port = config?.Port ?? 9087;
         options.ListenLocalhost(port, listenOptions =>
         {
            listenOptions.Protocols = HttpProtocols.Http2;
         });
      }
   });
}

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