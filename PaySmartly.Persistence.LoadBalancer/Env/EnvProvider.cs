using System.Text.Json;
using Microsoft.Extensions.Options;
using PaySmartly.Persistence.LoadBalancer.Env;

namespace PaySmartly.Persistence.LoadBalancer.Env
{
    public interface IEnvProvider
    {
        KestrelConfig? GetKestrelConfig();

        IEnumerable<string> GetPersistanceEndpointUrls();
    }

    public class EnvProvider(
        IOptions<KestrelConfig> kestrelSetting,
        IOptions<Endpoints> endpointsSetting
    ) : IEnvProvider
    {
        private const string KESTREL_ENDPOINT = "KESTREL_ENDPOINT_PORT";
        private const string PERSISTANCE_ENDPOINTS = "PERSISTANCE_ENDPOINTS";

        public KestrelConfig? GetKestrelConfig()
        {
            string? json = Environment.GetEnvironmentVariable(KESTREL_ENDPOINT);

            if (json is null)
            {
                return kestrelSetting?.Value;
            }
            else
            {
                JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
                var config = JsonSerializer.Deserialize<KestrelConfig>(json, options);
                return config;
            }
        }

        public IEnumerable<string> GetPersistanceEndpointUrls()
        {
            string? json = Environment.GetEnvironmentVariable(PERSISTANCE_ENDPOINTS);
            if (json is null)
            {
                return endpointsSetting?.Value?.Persistance ?? Enumerable.Empty<string>();
            }
            else
            {
                var urls = JsonSerializer.Deserialize<string[]>(json);
                return urls ?? Enumerable.Empty<string>();
            }
        }
    }
}