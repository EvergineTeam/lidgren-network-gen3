#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Lidgren.Network.Abstraction
{
    public class PlatformSocket
    {
        private Socket m_socket;

        public int Available
        {
            get
            {
                return this.m_socket.Available;
            }
        }

        public bool DontFragment
        {
            get
            {
                return this.m_socket.DontFragment;
            }
            set
            {
                this.m_socket.DontFragment = value;
            }
        }

        public bool IsBound
        {
            get
            {
                return this.m_socket.IsBound;
            }
        }

        public bool IsBroadcast
        {
            set
            {
                this.m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, value);
            }
        }

        public bool IsReuseAddress
        {
            set
            {
                this.m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value ? 1 : 0);
            }
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                return this.m_socket.LocalEndPoint;
            }
        }

        public int ReceiveBufferSize
        {
            get
            {
                return this.m_socket.ReceiveBufferSize;
            }
            set
            {
                this.m_socket.ReceiveBufferSize = value;
            }
        }

        public int SendBufferSize
        {
            get
            {
                return this.m_socket.SendBufferSize;
            }
            set
            {
                this.m_socket.SendBufferSize = value;
            }
        }

        public PlatformSocket()
        {
            this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Bind(IPAddress localAddress, int port)
        {
            this.m_socket.Blocking = false;

            var ep = (EndPoint)new IPEndPoint(localAddress, port);
            this.m_socket.Bind(ep);

            try
            {
                const uint IOC_IN = 0x80000000;
                const uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                this.m_socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            }
            catch
            {
                // ignore; SIO_UDP_CONNRESET not supported on this platform
            }
        }

        public void Shutdown()
        {
            this.m_socket.Shutdown(SocketShutdown.Receive);
#if  _NET_CORECLR
            this.m_socket.Dispose();
#else
            this.m_socket.Close(2); // 2 seconds timeout
#endif
        }

        public int IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue)
        {
            return this.m_socket.IOControl(ioControlCode, optionInValue, optionInValue);
        }

        public int SendTo(byte[] buffer, int offset, int size, IPEndPoint remoteEP)
        {
            return this.m_socket.SendTo(buffer, offset, size, SocketFlags.None, remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, int offset, int size, ref EndPoint remoteEP)
        {
            return this.m_socket.ReceiveFrom(buffer, offset, size, SocketFlags.None, ref remoteEP);
        }

        public bool Poll(int microSeconds)
        {
            return this.m_socket.Poll(microSeconds, SelectMode.SelectRead);
        }
    }
}
