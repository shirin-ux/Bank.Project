namespace LoanService.Api
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "192.168.87.12";
        public int Port { get; set; } = 5672;
        public string VirtualHost { get; set; } = "/";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";


        public string ExchangeName { get; set; } = "loan.notifications";
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Topic;
    }
}
