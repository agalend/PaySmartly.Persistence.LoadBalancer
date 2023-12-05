using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace PaySmartly.Persistence.LoadBalancer.ReverseProxy
{
    public class YarpProxyConfig : IProxyConfig
    {
        private readonly List<RouteConfig> routes;
        private readonly List<ClusterConfig> clusters;
        private readonly CancellationChangeToken changeToken;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public YarpProxyConfig()
        {
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
            var collection = new List<RouteConfig>
            {
                new()
                {
                    RouteId = "route1",
                    ClusterId = "cluster1",
                    Match = new RouteMatch()
                    {
                        Path = "{**catch-all}"
                    },
                    Transforms = new List<IReadOnlyDictionary<string, string>>()
                    {
                        new Dictionary<string, string> { {"PathPattern","{**catch-all}"}}
                    }
                }
            };

            return collection;
        }

        private List<ClusterConfig> GenerateClusters()
        {
            var collection = new List<ClusterConfig>
            {
                new()
                {
                    ClusterId = "cluster1",
                    HttpRequest= new ForwarderRequestConfig() {
                        VersionPolicy = HttpVersionPolicy.RequestVersionExact
                    },
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        {
                            "cluster1/destination1", new DestinationConfig()
                            {
                                Address = "http://localhost:9088/"
                            }
                        }
                    }
                }
            };

            return collection;
        }
    }
}