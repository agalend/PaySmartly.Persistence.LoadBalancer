using System;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PaySmartly.Persistence.LoadBalancer.Env;
using PaySmartly.Persistence.LoadBalancer.ReverseProxy;
using Yarp.ReverseProxy.Configuration;

string ServiceName = "Persistance.LoadBalancer Service";

var builder = WebApplication.CreateSlimBuilder(args);

IConfigurationSection endpointsSettings = builder.Configuration.GetSection("Endpoints");
builder.Services.Configure<Endpoints>(endpointsSettings);
IConfigurationSection kestrelSettings = builder.Configuration.GetSection("Kestrel");
builder.Services.Configure<KestrelConfig>(kestrelSettings);

KestrelConfig? kestrelConfig = kestrelSettings.Get<KestrelConfig>();
ConfigureKestrel(builder, EnvProvider.Instance.GetKestrelConfig(kestrelConfig));

AddOpenTelemetryLogging(builder);

builder.Services.AddSingleton<IEnvProvider, EnvProvider>();
builder.Services.AddTransient<IProxyConfigProvider, YarpProxyConfigProvider>();
builder.Services.AddReverseProxy();
AddOpenTelemetryService(builder);

var app = builder.Build();
app.MapReverseProxy();
app.Run();

void ConfigureKestrel(WebApplicationBuilder builder, KestrelConfig? config)
{
   builder.WebHost.ConfigureKestrel(options =>
   {
      int port = config?.Port ?? 9087;

      if (config?.ListenAnyIP == true)
      {
         options.Listen(IPAddress.Any, port, listenOptions =>
         {
            listenOptions.Protocols = HttpProtocols.Http2;
         });
      }
      else
      {
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