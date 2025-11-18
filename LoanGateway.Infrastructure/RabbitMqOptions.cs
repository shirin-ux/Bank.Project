using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Infrastructure;

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    // Exchange مخصوص نوتیفیکیشن‌ها
    public string ExchangeName { get; set; } = "loan.notifications";
    public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Topic;
}