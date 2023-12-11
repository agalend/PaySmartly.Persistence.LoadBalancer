using Microsoft.Extensions.Options;
using PaySmartly.Persistence.LoadBalancer.Env;
using Yarp.ReverseProxy.Configuration;

namespace PaySmartly.Persistence.LoadBalancer.ReverseProxy
{
    public class YarpProxyConfigProvider(
            IOptions<KestrelConfig> kestrelSetting,
            IOptions<Endpoints> endpointsSetting,
            IEnvProvider provider) : IProxyConfigProvider
    {
        private readonly IOptions<KestrelConfig> kestrelSetting = kestrelSetting;
        private readonly IOptions<Endpoints> endpointsSetting = endpointsSetting;
        private readonly IEnvProvider provider = provider;

        public IProxyConfig GetConfig()
        {
            return new YarpProxyConfig(kestrelSetting, endpointsSetting, provider);
        }
    }
}