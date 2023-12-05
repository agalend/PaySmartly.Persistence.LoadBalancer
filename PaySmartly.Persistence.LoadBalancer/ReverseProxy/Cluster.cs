namespace PaySmartly.Persistence.LoadBalancer.ReverseProxy
{
    public record Cluster(string Name, string Address, HealthCheck HealthCheck);
}