using System.Text.Json;

namespace PaySmartly.Persistence.LoadBalancer.Env
{
    public interface IEnvProvider
    {
        KestrelSettings? GetKestrelSettings();

        EndpointsSettings? GetEndpointsSettings();
    }

    public class EnvProvider(KestrelSettings? kestrelSettings, EndpointsSettings? endpointsSettings) : IEnvProvider
    {
        private const string KESTREL_ENDPOINT_PORT = "KESTREL_ENDPOINT_PORT";
        private const string PERSISTENCE_ENDPOINTS = "PERSISTENCE_ENDPOINTS";
        private readonly KestrelSettings? kestrelSettings = kestrelSettings;
        private readonly EndpointsSettings? endpointsSettings = endpointsSettings;

        public KestrelSettings? GetKestrelSettings()
        {
            string? strPort = Environment.GetEnvironmentVariable(KESTREL_ENDPOINT_PORT);

            if (!int.TryParse(strPort, out int port))
            {
                return kestrelSettings;
            }
            else
            {
                KestrelSettings config = new() { Port = port, ListenAnyIP = true };
                return config;
            }
        }

        public EndpointsSettings? GetEndpointsSettings()
        {
            string[]? persistenceUrls = endpointsSettings?.Persistence;
            string? json = Environment.GetEnvironmentVariable(PERSISTENCE_ENDPOINTS);

            if (json is null)
            {
                return new() { Persistence = persistenceUrls };
            }
            else
            {
                var urls = JsonSerializer.Deserialize<string[]>(json);
                return new() { Persistence = urls ?? persistenceUrls };
            }
        }
    }
}