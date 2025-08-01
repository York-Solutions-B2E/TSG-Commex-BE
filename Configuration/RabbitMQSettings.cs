// TSG-Commex-BE/Configuration/RabbitMQSettings.cs
namespace TSG_Commex_BE.Configuration;

public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "commex-events";
    public string QueueName { get; set; } = "communication-status-updates";
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int RequestedHeartbeat { get; set; } = 30;
}