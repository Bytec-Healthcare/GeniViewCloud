using GeniView.Cloud.Models;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using NLog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace GeniView.Cloud.Common
{
    public class MQTTHelper : IDisposable
    {
        private static readonly Lazy<MQTTHelper> _instance = new Lazy<MQTTHelper>(() => new MQTTHelper(null));
        public static MQTTHelper Instance => _instance.Value;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private string broker;
        private int port;
        private string clientId;
        private string userName;
        private string psw;
        private bool showDebugMsg = false;
        private bool isDispose = false;

        private IMqttClient _client;
        private MqttClientOptions _options;
        public MqttClientOptions Options { get => _options; set => _options = value; }

        private static string GetSetting(IConfiguration? configuration, params string[] keys)
        {
            if (configuration == null)
            {
                return string.Empty;
            }

            foreach (var key in keys)
            {
                var value = configuration[key];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }


        #region public functions
        public MQTTHelper(IConfiguration? configuration)
        {
            broker = GetSetting(configuration,
                "AppSettings:MQTTBroker",
                "MQTTBroker",
                "Mqtt:Broker",
                "MqttBroker");

            var portSetting = GetSetting(configuration,
                "AppSettings:MQTTPort",
                "MQTTPort",
                "Mqtt:Port",
                "MqttPort");

            clientId = GetSetting(configuration,
                "AppSettings:MQTTClientId",
                "MQTTClientId",
                "Mqtt:ClientId",
                "MqttClientId");

            userName = GetSetting(configuration,
                "AppSettings:MQTTUser",
                "MQTTUser",
                "Mqtt:User",
                "MqttUser");

            psw = GetSetting(configuration,
                "AppSettings:MQTTPSW",
                "MQTTPSW",
                "Mqtt:Password",
                "MqttPassword");

            if (string.IsNullOrWhiteSpace(broker))
            {
                broker = "localhost";
                _logger.Warn("MQTTBroker is not configured. Falling back to localhost.");
            }

            if (!int.TryParse(portSetting, out port))
            {
                port = 1883;
                _logger.Warn("MQTTPort is not configured or invalid. Falling back to 1883.");
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = "genicloud";
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = "geniviewuser";
            }

            if (string.IsNullOrWhiteSpace(psw))
            {
                psw = "G3niview!@#?";
            }

            _logger.Info($"MQTT config resolved. Broker={broker}, Port={port}, ClientId={clientId}");

            try
            {
                // Create a new MQTT client.
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                // Configure options for the client.
                Options = new MqttClientOptionsBuilder()
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                .WithClientId(clientId)
                .WithCredentials(userName, psw)
                .WithTcpServer(broker, port) // Use TCP connection.
                .WithSessionExpiryInterval(uint.MaxValue)
                .WithCleanSession(false)    // Very important, it will affect the QoS receive message flow.
                //.WithKeepAlivePeriod(new TimeSpan(0, 0, 60))
                //.WithTimeout(new TimeSpan(0, 0, 30))
                .Build();


                // Setup connecting event handler.
                _client.ConnectingAsync += mqttClient_ConnectingAsync;

                // Setup connected event handler.
                _client.ConnectedAsync += mqttClient_ConnectedAsync;

                // Setup disconnected event handler.
                _client.DisconnectedAsync += mqttClient_DisconnectedAsync;

                // Setup receive message event handler.
                _client.ApplicationMessageReceivedAsync += mqttClient_MessageReceivedAsync;

            }
            catch (AggregateException aggEx)
            {
                foreach (var ex in aggEx.InnerExceptions)
                {
                    _logger.Error($"AggregateException : MQTT Client create failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($" MQTT Client create failed: {ex.Message}");

            }
        }

        public async Task Connect()
        {
            try
            {
                // Connect to the MQTT broker.
                await _client.ConnectAsync(Options);

            }
            catch (AggregateException aggEx)
            {
                foreach (var ex in aggEx.InnerExceptions)
                {
                    _logger.Error($"Connection to MQTT broker failed: {ex.Message}");

                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Connection to MQTT broker failed: {ex.Message}");

            }
        }

        public void Disconnect()
        {
            if (_client.IsConnected)
            {
                //Disconnect from the MQTT broker.
                _client.DisconnectAsync().Wait();
            }
        }

        public void Subscribe(string topic, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (!_client.IsConnected)
            {
                _logger.Warn("Client Subscribe : Client does not connect to broker");

                return;
            }
            else
            {
                // Subscribe topic from the MQTT broker.
                _logger.Info($"Client Subscribe : Client Subscribe topic({topic}) from the MQTT broker");
                _client.SubscribeAsync(topic, qosLevel);
            }
        }

        public void Unsubscribe(string topic)
        {
            if (!_client.IsConnected)
            {
                _logger.Warn("Client Unsubscribe : Client does not connect to broker");
                return;
            }
            else
            {
                // Unsubscribe topic from the MQTT broker.
                _logger.Info($"Client Unsubscribe : Client Unsubscribe topic({topic}) from the MQTT broker");
                _client.UnsubscribeAsync(topic);
            }
        }

        public void Publish(string topic, string data, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtMostOnce, bool retain = true)
        {
            if (!_client.IsConnected)
            {
                _logger.Warn("Client Publish : Client does not connect to broker");
                return;
            }
            else
            {
                // Publish data to the MQTT broker.
                _logger.Debug($"Client Publish : Topic={topic}, Payload={data} to broker");
                _client.PublishStringAsync(topic, data, qosLevel, retain);
            }

        }

        //public async Task Publish(string topic, string data, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtMostOnce)
        public async Task<MqttClientPublishResult> PublishAsync(string topic, string data, MqttQualityOfServiceLevel qosLevel = MqttQualityOfServiceLevel.AtMostOnce, bool retain = true)

        {
            var result = await _client.PublishStringAsync(topic, data, qosLevel, retain);
            _logger.Debug($"Client Publish : Topic={topic}, Payload={data} to broker");
            return result;
        }

        public void Dispose()
        {
            if (_client.IsConnected)
            {
                isDispose = true;
                _client.Dispose();
            }
            _logger.Warn($"Client dispose");

        }
        #endregion

        #region event
        private Task mqttClient_ConnectingAsync(MqttClientConnectingEventArgs arg)
        {
            _logger.Info("Client Connecting MQTT broker");
            return Task.CompletedTask;
        }
        private async Task mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            _logger.Info("Client Connected MQTT broker");
            foreach (var topic in MQTTTopic.Topics)
            {
                await _client.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtLeastOnce);
                _logger.Info($"Client Subscribe : Topic={topic}, QoS=AtLeastOnce");
            }
        }
        private async Task mqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            _logger.Warn("Client Disconnected MQTT broker");

            try
            {
                if (isDispose == false)
                {
                    _logger.Warn("Client Reconnecting MQTT broker");

                    await Task.Delay(new TimeSpan(0, 0, 10));  // Code delay for testing disconnect 10 seconds then reconnect.
                    await _client.ConnectAsync(Options);
                }


            }
            catch (Exception ex)
            {
                _logger.Error("Mqtt reconnecting failed", ex);
            }
        }
        private Task mqttClient_MessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            string topic = arg.ApplicationMessage.Topic;
            if (arg.ApplicationMessage.Payload != null)
            {
                var msg = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload).Replace("\t", "").Replace("\n", "");

                if (arg.ApplicationMessage.Retain == true)
                {
                    _logger.Info($"Client Received Retain Packet : Topic={topic}, PayloadLength={msg.Length}");
                }
                else
                {
                    Global._queueHelp.Enqueue(arg.ApplicationMessage);
                    _logger.Info($"Client Received : Topic={topic}, PayloadLength={msg.Length}, QueueCount={Global._queueHelp._queue.Count}");
                }
            }

            return Task.CompletedTask;
        }
        #endregion

    }
}