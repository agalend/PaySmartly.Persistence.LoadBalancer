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

string ServiceName = "Persistence.LoadBalancer Service";

var builder = WebApplication.CreateSlimBuilder(args);

IEnvProvider envProvider = CreateEnvProvider(builder);
ConfigureKestrel(builder, envProvider.GetKestrelSettings());
AddOpenTelemetryLogging(builder);

builder.Services.AddSingleton(envProvider);
builder.Services.AddSingleton<IProxyConfigProvider, YarpProxyConfigProvider>();
builder.Services.AddReverseProxy();
AddOpenTelemetryService(builder);

var app = builder.Build();
app.MapReverseProxy();
app.Run();

void ConfigureKestrel(WebApplicationBuilder builder, KestrelSettings? config)
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

IEnvProvider CreateEnvProvider(WebApplicationBuilder builder)
{
   IConfigurationSection endpointsSection = builder.Configuration.GetSection("Endpoints");
   EndpointsSettings? endpointsSettings = endpointsSection.Get<EndpointsSettings>();
   IConfigurationSection kestrelSection = builder.Configuration.GetSection("Kestrel");
   KestrelSettings? kestrelSettings = kestrelSection.Get<KestrelSettings>();

   EnvProvider envProvider = new(kestrelSettings, endpointsSettings);
   return envProvider;
}