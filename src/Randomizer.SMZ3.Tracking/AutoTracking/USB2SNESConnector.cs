using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Auto Tracker connector for USB2SNES. Opens a websocket client to the USB2SNES
    /// server of ws://localhost:8080.
    /// Docs: https://github.com/Skarsnik/QUsb2snes/blob/master/docs/Procotol.md
    /// </summary>
    public class USB2SNESConnector : IEmulatorConnector
    {
        private readonly ILogger _logger;
        private bool _isConnected;
        private readonly WebsocketClient _client;
        private EmulatorAction? _lastMessage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public USB2SNESConnector(ILogger<USB2SNESConnector> logger)
        {
            _logger = logger;

            _lastMessage = null;

            var url = new Uri("ws://localhost:8080");

            _client = new WebsocketClient(url);
            _client.ReconnectTimeout = TimeSpan.FromSeconds(5);

            _client.ReconnectionHappened.Subscribe(info =>
            {
                _lastMessage = null;
                _logger.LogError($"Reconnection happened, type: {info.Type}");

                Thread.Sleep(1000);

                _logger.LogInformation($"Requesting device list");

                Task.Run(() => _client.Send(JsonSerializer.Serialize(new USB2SNESRequest()
                {
                    Opcode = "DeviceList",
                    Space = "SNES"
                })));
            });

            _client.DisconnectionHappened.Subscribe(info =>
            {
                _lastMessage = null;
                _logger.LogError(info.Exception, $"USB2SNES connection lost {info.Type}");
                if (_isConnected)
                {
                    _isConnected = false;
                    OnDisconnected?.Invoke(this, new());
                }
            });

            _client.MessageReceived.Subscribe(msg => HandleUSB2SNESResponse(msg));
            _client.Start();
        }

        /// <summary>
        /// When a connection with the emulator is established
        /// </summary>
        public event EventHandler? OnConnected;

        /// <summary>
        /// When the connection with the emulator has been lost
        /// </summary>
        public event EventHandler? OnDisconnected;

        /// <summary>
        /// When a message from the emulator with memory has been returned
        /// </summary>
        public event EmulatorDataReceivedEventHandler? MessageReceived;

        /// <summary>
        /// If the connector is currently connected to the emulator
        /// </summary>
        /// <returns></returns>
        public bool IsConnected() => _isConnected;

        /// <summary>
        /// Sends a message to the emulator
        /// </summary>
        /// <param name="message">The message to send to the emulator</param>
        public void SendMessage(EmulatorAction message)
        {
            if (_lastMessage != null) return;

            _logger.LogTrace("Sending " + message.Type.ToString());

            var address = TranslateAddress(message).ToString("X");
            var length = message.Length.ToString("X");

            if (message.Type == EmulatorActionType.ReadBlock)
            {
                _lastMessage = message;
                _client.Send(JsonSerializer.Serialize(new USB2SNESRequest()
                {
                    Opcode = "GetAddress",
                    Space = "SNES",
                    Operands = new List<string>() { address, length }
                }));
            }
            else if (message.Type == EmulatorActionType.WriteBytes && message.WriteValues != null)
            {
                _ = SendBinaryDataAsync(address, message.WriteValues.ToArray());
            }

        }

        private async Task SendBinaryDataAsync(string address, byte[] data)
        {
            var message = JsonSerializer.Serialize(new USB2SNESRequest()
            {
                Opcode = "PutAddress",
                Space = "SNES",
                Operands = new List<string>() { address, data.Length.ToString("X") }
            });

            _client.Send(message);

            // Wait to make sure both messages send in the appropriate order
            // as USB2SNES needs to receive the PutAddress OpCode first
            await Task.Delay(TimeSpan.FromSeconds(0.05));

            _client.Send(data);
        }

        /// <summary>
        /// Closes the connection so that the connector can be destroyed
        /// </summary>
        public void Dispose()
        {
            try
            {
                _client.Dispose();
            }
            catch (ObjectDisposedException) { }
            GC.SuppressFinalize(this);
        }

        private void HandleUSB2SNESResponse(ResponseMessage msg)
        {
            // For text responses it should be the device info, so we need to attach ourselves to that device
            if (msg.MessageType == WebSocketMessageType.Text)
            {
                _logger.LogTrace($"Receiving text data: {msg.Text}");

                var response = JsonSerializer.Deserialize<USB2SNESResponse>(msg.Text);
                if (response == null || response.Results == null || response.Results.Count == 0)
                {
                    _logger.LogError("Invalid json response " + WebSocketMessageType.Text);
                    return;
                }

                _logger.LogInformation($"Connecting to USB2SNES device {response.Results.FirstOrDefault()}");

                _client.Send(JsonSerializer.Serialize(new USB2SNESRequest()
                {
                    Opcode = "Attach",
                    Space = "SNES",
                    Operands = new List<string>() { response.Results.FirstOrDefault() ?? "" }
                }));

                _client.Send(JsonSerializer.Serialize(new USB2SNESRequest()
                {
                    Opcode = "Name",
                    Space = "SNES",
                    Operands = new List<string>() { "SMZ3 Tracker" }
                }));

                // Wait a second before showing as connecting to avoid issues with reading memory too early
                Task.Run(new Action(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    _isConnected = true;
                    OnConnected?.Invoke(this, new());
                }));

            }
            // If it's a binary response, find the last requested message to use it as the address since
            // USB2SNES returns with NO context for what this is a response to...
            else if (msg.MessageType == WebSocketMessageType.Binary && msg.Binary != null)
            {
                var data = new EmulatorMemoryData(msg.Binary);

                if (_lastMessage != null)
                {
                    _logger.LogTrace($"Receiving {_lastMessage.Type} unknown binary data");
                    MessageReceived?.Invoke(this, new(_lastMessage.Address, data));
                    _lastMessage = null;
                }
                else
                {
                    _logger.LogTrace("Receiving unknown binary data");
                }
            }
        }

        /// <summary>
        /// Returns if the connector is ready for another message to be sent
        /// </summary>
        /// <returns>True if the connector is ready for another message, false otherwise</returns>
        public bool CanSendMessage() => _lastMessage == null;

        /// <summary>
        /// USB2SNES (and Bizhawk) have the SRAM/CartRAM
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Translated address</returns>
        private int TranslateAddress(EmulatorAction message)
        {
            if (message.Domain == MemoryDomain.CartROM)
            {
                return message.Address;
            }
            else if (message.Domain == MemoryDomain.CartRAM)
            {
                var offset = 0x0;
                var remaining = message.Address - 0xa06000;
                while (remaining >= 0x2000)
                {
                    remaining -= 0x10000;
                    offset += 0x2000;
                }
                return 0xE00000 + offset + remaining;
            }
            else if (message.Domain == MemoryDomain.WRAM)
            {
                return message.Address + 0x770000;
            }
            return message.Address;
        }
    }
}
