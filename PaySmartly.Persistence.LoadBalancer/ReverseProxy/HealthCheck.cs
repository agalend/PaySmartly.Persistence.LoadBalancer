namespace PaySmartly.Persistence.LoadBalancer.ReverseProxy
{
    public record HealthCheck(string Interval, string Timeout, string Address, string Path);
}