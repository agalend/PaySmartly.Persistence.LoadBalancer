using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using PaySmartly.Persistence.LoadBalancer.Env;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.LoadBalancing;

namespace PaySmartly.Persistence.LoadBalancer.ReverseProxy
{
    public class YarpProxyConfig : IProxyConfig
    {
        private readonly IOptions<KestrelConfig> kestrelSetting;
        private readonly IOptions<Endpoints> endpointsSetting;
        private readonly IEnvProvider provider;
        private readonly List<RouteConfig> routes;
        private readonly List<ClusterConfig> clusters;
        private readonly CancellationChangeToken changeToken;
        private readonly CancellationTokenSource cts;

        public YarpProxyConfig(
            IOptions<KestrelConfig> kestrelSetting,
            IOptions<Endpoints> endpointsSetting,
            IEnvProvider provider)
        {
            this.kestrelSetting = kestrelSetting;
            this.endpointsSetting = endpointsSetting;
            this.provider = provider;

            routes = GenerateRoutes();
            clusters = GenerateClusters();

            cts = new CancellationTokenSource();
            changeToken = new CancellationChangeToken(cts.Token);
        }

        public IReadOnlyList<RouteConfig> Routes => routes;
        public IReadOnlyList<ClusterConfig> Clusters => clusters;
        public IChangeToken ChangeToken => changeToken;

        private List<RouteConfig> GenerateRoutes()
        {
            var collection = new List<RouteConfig>() { CreateRoute(1) };
            return collection;
        }

        private List<ClusterConfig> GenerateClusters()
        {
            IEnumerable<string> urls = provider.GetPersistanceEndpointUrls(endpointsSetting?.Value);
            ClusterConfig config = CreateCluster(1, urls);
            var collection = new List<ClusterConfig>([config]);

            return collection;
        }

        private RouteConfig CreateRoute(int number)
        {
            return new()
            {
                RouteId = $"route{number}",
                ClusterId = $"cluster{number}",
                Match = new RouteMatch()
                {
                    Path = "{**catch-all}"
                },
                Transforms = new List<IReadOnlyDictionary<string, string>>()
                {
                    new Dictionary<string, string> { {"PathPattern","{**catch-all}"}}
                }
            };
        }

        private ClusterConfig CreateCluster(int number, IEnumerable<string> urls)
        {
            Dictionary<string, DestinationConfig> destinations = [];

            int count = 1;
            foreach (var url in urls)
            {
                destinations.Add($"cluster{number}/destination{count}", new DestinationConfig()
                {
                    Address = url,
                    Health = url
                });

                count += 1;
            }

            return new()
            {
                ClusterId = $"cluster{number}",
                HealthCheck = new()
                {
                    Active = new()
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(1),
                        Timeout = TimeSpan.FromSeconds(1),
                        Policy = "ConsecutiveFailures",
                        Path = "/health"
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    {"ConsecutiveFailuresHealthPolicy.Threshold", "1"}
                },
                LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
                HttpRequest = new ForwarderRequestConfig()
                {
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact
                },
                Destinations = destinations
            };
        }
    }
}