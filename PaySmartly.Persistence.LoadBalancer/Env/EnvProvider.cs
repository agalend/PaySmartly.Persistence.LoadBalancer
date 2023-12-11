using System.Text.Json;

namespace PaySmartly.Persistence.LoadBalancer.Env
{
    public interface IEnvProvider
    {
        KestrelConfig? GetKestrelConfig(KestrelConfig? kestrelSetting);

        IEnumerable<string> GetPersistanceEndpointUrls(Endpoints? endpointsSetting);
    }

    public class EnvProvider : IEnvProvider
    {
        private const string KESTREL_ENDPOINT_PORT = "KESTREL_ENDPOINT_PORT";
        private const string PERSISTANCE_ENDPOINTS = "PERSISTANCE_ENDPOINTS";

        public static readonly IEnvProvider Instance = new EnvProvider();

        public KestrelConfig? GetKestrelConfig(KestrelConfig? kestrelSetting)
        {
            string? strPort = Environment.GetEnvironmentVariable(KESTREL_ENDPOINT_PORT);

            if (!int.TryParse(strPort, out int port))
            {
                return kestrelSetting;
            }
            else
            {
                KestrelConfig config = new() { Port = port, ListenAnyIP = true };
                return config;
            }
        }

        public IEnumerable<string> GetPersistanceEndpointUrls(Endpoints? endpointsSetting)
        {
            string? json = Environment.GetEnvironmentVariable(PERSISTANCE_ENDPOINTS);
            if (json is null)
            {
                return endpointsSetting?.Persistance ?? Enumerable.Empty<string>();
            }
            else
            {
                var urls = JsonSerializer.Deserialize<string[]>(json);
                return urls ?? Enumerable.Empty<string>();
            }
        }
    }
}