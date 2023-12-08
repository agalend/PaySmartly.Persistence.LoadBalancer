using System.Text.Json;
using PaySmartly.Persistence.LoadBalancer.Env;

namespace PaySmartly.Persistence.LoadBalancer
{
    public interface IEnvProvider
    {
        KestrelConfig? GetKestrelConfig();

        IEnumerable<string> GetPersistanceEndpointUrls();
    }

    public class EnvProvider : IEnvProvider
    {
        private const string KESTREL_ENDPOINT = "KESTREL_ENDPOINT_PORT";
        private const string DEFAULT_KESTREL_ENDPOINT = "{\"port\": 9087, \"listenAnyIP\": false}";

        private const string PERSISTANCE_ENDPOINTS = "PERSISTANCE_ENDPOINTS";
        private const string DEFAULTS_PERSISTANCE_ENDPOINTS = "[\"http://localhost:9088/\", \"http://localhost:9089/\"]";

        public static readonly IEnvProvider Instance;

        static EnvProvider() => Instance = new EnvProvider();

        public KestrelConfig? GetKestrelConfig()
        {
            string? json = Environment.GetEnvironmentVariable(KESTREL_ENDPOINT);
            json ??= DEFAULT_KESTREL_ENDPOINT;

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
            var config = JsonSerializer.Deserialize<KestrelConfig>(json, options);
            return config;
        }

        public IEnumerable<string> GetPersistanceEndpointUrls()
        {
            string? json = Environment.GetEnvironmentVariable(PERSISTANCE_ENDPOINTS);
            string endpoint = json ?? DEFAULTS_PERSISTANCE_ENDPOINTS;


            List<string> result = [];
            var urls = JsonSerializer.Deserialize<string[]>(endpoint);

            if (urls is not null)
            {
                foreach (var url in urls)
                {
                    result.Add(url);
                }
            }

            return result;
        }
    }
}