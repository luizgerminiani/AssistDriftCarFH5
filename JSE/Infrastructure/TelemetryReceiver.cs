using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JSE.Core;

namespace JSE.Infrastructure
{
    public sealed class TelemetryReceiver : IDisposable
    {
        private readonly int _port;
        private UdpClient _udp;
        private Thread _thread;
        private volatile bool _running;
        private TelemetryFrame _latest;
        private long _packetCount;
        private DateTime _lastPacketUtc;

        public TelemetryReceiver(int port) => _port = port;
        public TelemetryFrame Latest => _latest;
        public long PacketCount => Interlocked.Read(ref _packetCount);
        public bool IsReceiving => (DateTime.UtcNow - _lastPacketUtc).TotalSeconds < 1.0;

        public void Start()
        {
            if (_running) return;
            _udp = new UdpClient(new IPEndPoint(IPAddress.Loopback, _port));
            _running = true;
            _thread = new Thread(ReceiveLoop) { IsBackground = true, Name = "FH5 UDP telemetry" };
            _thread.Start();
        }

        private void ReceiveLoop()
        {
            var sender = new IPEndPoint(IPAddress.Any, 0);
            while (_running)
            {
                try
                {
                    byte[] bytes = _udp.Receive(ref sender);
                    if (ForzaHorizon5Packet.TryParse(bytes, out TelemetryFrame frame))
                    {
                        _latest = frame;
                        _lastPacketUtc = DateTime.UtcNow;
                        Interlocked.Increment(ref _packetCount);
                    }
                }
                catch (SocketException) when (!_running) { }
                catch (ObjectDisposedException) when (!_running) { }
            }
        }

        public void Dispose()
        {
            _running = false;
            _udp?.Close();
            _thread?.Join(500);
            _udp?.Dispose();
        }
    }
}
