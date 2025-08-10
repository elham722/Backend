namespace Backend.Infrastructure.ExternalServices
{
    public class ExternalServiceOptions
    {
        public string BaseUrl { get; set; } = "https://localhost:7223";
        public int TimeoutSeconds { get; set; } = 30;
        public bool UseHttps { get; set; } = true;
        public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    }
} 