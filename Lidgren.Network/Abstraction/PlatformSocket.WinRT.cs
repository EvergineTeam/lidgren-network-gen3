#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endregion

namespace Lidgren.Network.Abstraction
{
    public class PlatformSocket
    {
        private const int STREAMS_CACHE_MAX_SIZE = 100;

        private DatagramSocket m_socket;
        private Queue<Tuple<IPEndPoint, byte[]>> m_datagrams;
        private int dataAvailable;
        private bool isBound;
        private ManualResetEvent dataAvailableEvent;
        private Dictionary<IPEndPoint, DataWriter> sendStreams;

        public int Available
        {
            get
            {
                return dataAvailable;
            }
        }

        public bool DontFragment
        {
            set
            {
#if NETFX_CORE
                m_socket.Control.DontFragment = value;
#endif
            }
        }

        public bool IsBound
        {
            get
            {                
                return isBound;
            }
        }

        public bool IsBroadcast
        {
            set
            {
            }
        }

        public bool IsReuseAddress
        {
            set
            {
            }
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                return new IPEndPoint(IPAddress.Parse(m_socket.Information.LocalAddress.CanonicalName), int.Parse(m_socket.Information.LocalPort));
            }
        }

        public int ReceiveBufferSize
        {
            set
            {
            }
        }

        public int SendBufferSize
        {
            set
            {
            }
        }

        public PlatformSocket()
        {
            m_socket = new DatagramSocket();
            m_datagrams = new Queue<Tuple<IPEndPoint, byte[]>>();
            dataAvailableEvent = new ManualResetEvent(false);
            sendStreams = new Dictionary<IPEndPoint, DataWriter>();
            m_socket.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                var remoteEP = new IPEndPoint(IPAddress.Parse(args.RemoteAddress.ToString()), int.Parse(args.RemotePort));
                var reader = args.GetDataReader();

                int receivedByteCount = (int)reader.UnconsumedBufferLength;
                if (receivedByteCount > 0)
                {
                    var resultBytes = new byte[receivedByteCount];
                    reader.ReadBytes(resultBytes);
                    m_datagrams.Enqueue(new Tuple<IPEndPoint, byte[]>(remoteEP, resultBytes));
                    Interlocked.Add(ref dataAvailable, receivedByteCount);
                    dataAvailableEvent.Set();
                }
            }
            catch (Exception ex)
            {
                SocketErrorStatus status = Windows.Networking.Sockets.SocketError.GetStatus(ex.HResult);

                if (status != SocketErrorStatus.ConnectionResetByPeer)
                {
                    throw ex;
                }
            }
        }

        public void Bind(IPAddress localAddress, int port)
        {
            m_socket.BindServiceNameAsync(port.ToString()).AsTask().Wait();

            isBound = true;
        }

        public void Shutdown()
        {
            if (m_socket != null)
            {
                isBound = false;

                m_socket.Dispose();
            }
        }

        public int SendTo(byte[] buffer, int offset, int size, IPEndPoint remoteEP)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            var content = buffer;
            if (buffer.Length != size)
            {
                content = new byte[size];
                Array.Copy(buffer, offset, content, 0, size);
            }

            try
            {
                var stream = GetStream(remoteEP);
                stream.WriteBytes(content);
                var res = stream.StoreAsync().AsTask().Result;
                return (int)res;
            }
            catch
            {
                sendStreams.Remove(remoteEP);
                throw;
            }
        }

        private DataWriter GetStream(IPEndPoint remoteEP)
        {
            DataWriter stream;
            if (!sendStreams.TryGetValue(remoteEP, out stream))
            {
                if(sendStreams.Count >= STREAMS_CACHE_MAX_SIZE)
                {
                    sendStreams.Remove(sendStreams.Keys.First());
                }

                var hostName = new HostName(remoteEP.Address.ToString());
                var port = remoteEP.Port.ToString();
                stream = new DataWriter(m_socket.GetOutputStreamAsync(hostName, port).AsTask().Result);
                sendStreams[remoteEP] = stream;
            }
            return stream;
        }

        public int ReceiveFrom(byte[] buffer, int offset, int size, ref EndPoint remoteEP)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (remoteEP == null || !(remoteEP is IPEndPoint))
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            if (dataAvailable > 0)
            {
                var datagram = m_datagrams.Peek();

                remoteEP = datagram.Item1;
                var data = datagram.Item2;
                var datagramLength = (int)data.Length;
                var readBytes = datagramLength > size ? size : datagramLength;
                Array.Copy(data, 0, buffer, offset, readBytes);

                if (size < datagramLength)
                {
                    throw new SocketException();
                }

                m_datagrams.Dequeue();
                Interlocked.Add(ref dataAvailable, -datagramLength);

                return readBytes;
            }

            return 0;
        }

        public bool Poll(int microSeconds)
        {
            if (this.Available <= 0)
            {
                dataAvailableEvent.WaitOne(microSeconds / 1000);
            }

            return this.Available > 0;
        }
    }
}
