namespace Client.MVC.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? Message { get; set; }
        public string? ExceptionType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool ShowMessage => !string.IsNullOrEmpty(Message);
        public bool ShowExceptionType => !string.IsNullOrEmpty(ExceptionType);
    }
}
