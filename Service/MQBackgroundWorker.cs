using System.Text;
using System.Text.Json;
using EmailTriggerApp.Model;
using EmailTriggerApp.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;

public class MQBackgroundWorker : BackgroundService
{
    private readonly HiveMQSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MQBackgroundWorker> _logger;

    public MQBackgroundWorker(
        IOptions<HiveMQSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<MQBackgroundWorker> logger)
    {
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        // 1. Message Handling with Exception Safety and Proper Scoping
        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var topic = e.ApplicationMessage.Topic;

                // MQTTnet helper method (or use ConvertPayloadToString())
                string json = e.ApplicationMessage.ConvertPayloadToString();

                _logger.LogInformation("Topic: {Topic}", topic);
                _logger.LogInformation("JSON: {Json}", json);

                if (!string.IsNullOrEmpty(json))
                {
                    var emailMessage = JsonSerializer.Deserialize<EmailMessage>(json);

                    if (emailMessage != null)
                    {
                        // Create a fresh scope for each incoming message
                        using var scope = _scopeFactory.CreateScope();
                        var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

                        await messageService.SendAsync(topic, emailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions in events cause silent failures—log them here
                _logger.LogError(ex, "Error processing incoming MQTT message.");
            }
        };

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.Host, _settings.Port)
            .WithCredentials(_settings.Username, _settings.Password)
            .WithCleanSession()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
            .WithTlsOptions(tls => tls.UseTls())
            .Build();

        // 2. Continuous Connection & Subscription Loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!mqttClient.IsConnected)
                {
                    _logger.LogInformation("Connecting to MQTT Broker...");
                    await mqttClient.ConnectAsync(options, stoppingToken);

                    // Re-subscribe whenever reconnected
                    await mqttClient.SubscribeAsync(
                        new MqttTopicFilterBuilder().WithTopic("#").Build(),
                        stoppingToken
                    );
                    _logger.LogInformation("Successfully connected and subscribed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker. Retrying in 5 seconds...");
            }

            // Check connection status every 5 seconds
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}