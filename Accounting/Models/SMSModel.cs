namespace Accounting.Models
{
    public class SMSModel
    {
        public long? Destination { get; set; }
        public SMSOptions? Options { get; set; }
    }

    public class SMSOptions
    {
        public string? Host { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public long? Source { get; set; }
        public string? Message { get; set; }
    }
}
