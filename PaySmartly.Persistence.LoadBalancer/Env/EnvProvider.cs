using System.Text.Json;

namespace PaySmartly.Persistence.LoadBalancer.Env
{
    public interface IEnvProvider
    {
        KestrelSettings? GetKestrelSettings(KestrelSettings? defaultSettings);

        EndpointsSettings? GetEndpointsSettings(EndpointsSettings? defaultSettings);
    }

    public class EnvProvider : IEnvProvider
    {
        private const string KESTREL_ENDPOINT_PORT = "KESTREL_ENDPOINT_PORT";
        private const string PERSISTENCE_ENDPOINTS = "PERSISTENCE_ENDPOINTS";

        public static readonly IEnvProvider Instance = new EnvProvider();

        public KestrelSettings? GetKestrelSettings(KestrelSettings? defaultSettings)
        {
            string? strPort = Environment.GetEnvironmentVariable(KESTREL_ENDPOINT_PORT);

            if (!int.TryParse(strPort, out int port))
            {
                return defaultSettings;
            }
            else
            {
                KestrelSettings config = new() { Port = port, ListenAnyIP = true };
                return config;
            }
        }

        public EndpointsSettings? GetEndpointsSettings(EndpointsSettings? defaultSettings)
        {
            string? json = Environment.GetEnvironmentVariable(PERSISTENCE_ENDPOINTS);
            if (json is null)
            {
                string[] persistenceUrls = defaultSettings?.Persistence ?? [];
                return new() { Persistence = persistenceUrls };
            }
            else
            {
                var urls = JsonSerializer.Deserialize<string[]>(json);
                string[] persistenceUrls = urls ?? [];
                return new() { Persistence = persistenceUrls };
            }
        }
    }
}