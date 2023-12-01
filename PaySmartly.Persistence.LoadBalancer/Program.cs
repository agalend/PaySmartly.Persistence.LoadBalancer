var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
   .AddReverseProxy()
   .LoadFromConfig(builder.Configuration.GetSection("LoadBalancer"));

var app = builder.Build();
app.MapReverseProxy();
app.Run();