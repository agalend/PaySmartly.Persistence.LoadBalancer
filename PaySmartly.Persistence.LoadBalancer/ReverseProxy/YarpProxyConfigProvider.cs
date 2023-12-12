using Microsoft.Extensions.Options;
using PaySmartly.Persistence.LoadBalancer.Env;
using Yarp.ReverseProxy.Configuration;

namespace PaySmartly.Persistence.LoadBalancer.ReverseProxy
{
    public class YarpProxyConfigProvider(
            IOptions<KestrelSettings> kestrelSetting,
            IOptions<EndpointsSettings> endpointsSetting,
            IEnvProvider provider) : IProxyConfigProvider
    {
        private readonly IOptions<KestrelSettings> kestrelSetting = kestrelSetting;
        private readonly IOptions<EndpointsSettings> endpointsSetting = endpointsSetting;
        private readonly IEnvProvider provider = provider;

        public IProxyConfig GetConfig()
        {
            return new YarpProxyConfig(kestrelSetting, endpointsSetting, provider);
        }
    }
}