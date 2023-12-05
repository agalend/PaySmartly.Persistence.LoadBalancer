using Yarp.ReverseProxy.Configuration;

namespace PaySmartly.Persistence.LoadBalancer.ReverseProxy
{
    public class YarpProxyConfigProvider : IProxyConfigProvider
    {
        public IProxyConfig GetConfig()
        {
            return new YarpProxyConfig();
        }
    }
}