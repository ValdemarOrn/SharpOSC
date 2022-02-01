using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SharpOSC
{
	public delegate void ProxyOscPacket(OscPacket packet);
	public delegate void ProxyBytePacket(byte[] packet);

	/// <summary>
	/// UDP connection for bidirectional use cases. 
	/// </summary>
	public class UDProxy : IDisposable
	{
		public int remote_Port { get; private set; }
		public int local_Port { get; private set; }

		object callbackLock;

		UdpClient proxyUdpClient;
		IPEndPoint RemoteIpEndPoint;
		IPEndPoint RemoteIpRxEndPoint;

		ProxyBytePacket BytePacketCallback = null;
		ProxyOscPacket OscPacketCallback = null;

		Queue<byte[]> queue;
		ManualResetEvent ClosingEvent;

		public UDProxy(string remote_address, int remote_port, int listening_port)
		{
			remote_Port = remote_port;
			local_Port = listening_port;
			
			queue = new Queue<byte[]>();
			ClosingEvent = new ManualResetEvent(false);
			callbackLock = new object();

			// try to open the port 10 times, else fail
			for (int i = 0; i < 10; i++)
			{
				try
				{
					proxyUdpClient = new UdpClient(listening_port);
					break;
				}
				catch (Exception)
				{
					// Failed in ten tries, throw the exception and give up
					if (i >= 9)
						throw;

					Thread.Sleep(5);
				}
			}

			IPAddress[] addresses = Dns.GetHostAddresses(remote_address);

			RemoteIpEndPoint = new IPEndPoint(addresses[0], remote_port);
			RemoteIpRxEndPoint = new IPEndPoint(addresses[0], 0);

			//proxyUdpClient.Connect(RemoteIpEndPoint);

			// setup first async event
			AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
			proxyUdpClient.BeginReceive(callBack, null);
		}

		public UDProxy(string remote_address, int remote_port, int listening_port, ProxyOscPacket callback) : this(remote_address, remote_port, listening_port)
		{
			this.OscPacketCallback = callback;
		}

		public UDProxy(string remote_address, int remote_port, int listening_port, ProxyBytePacket callback) : this(remote_address, remote_port, listening_port)
		{
			this.BytePacketCallback = callback;
		}

		void ReceiveCallback(IAsyncResult result)
		{
			Monitor.Enter(callbackLock);
			Byte[] bytes = null;

			try
			{
				bytes = proxyUdpClient.EndReceive(result, ref RemoteIpRxEndPoint);
			}
			catch (ObjectDisposedException e)
			{
				// Ignore if disposed. This happens when closing the listener
				return;
			} catch (SocketException e)
            {
				// Ignore. This happens when at this moment no remote receiver exists.
				return;
			}

			// Process bytes
			if (bytes != null && bytes.Length > 0)
			{
				if (BytePacketCallback != null)
				{
					BytePacketCallback(bytes);
				}
				else if (OscPacketCallback != null)
				{
					OscPacket packet = null;
					try
					{
						packet = OscPacket.GetPacket(bytes);
					}
					catch (Exception e)
					{
						// If there is an error reading the packet, null is sent to the callback
					}

					OscPacketCallback(packet);
				}
				else
				{
					lock (queue)
					{
						queue.Enqueue(bytes);
					}
				}
			}

			if (closing)
				ClosingEvent.Set();
			else
			{
				// Setup next async event
				AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
				proxyUdpClient.BeginReceive(callBack, null);
			}
			Monitor.Exit(callbackLock);
		}

		public void Send(byte[] message)
		{
			proxyUdpClient.Send(message, message.Length, RemoteIpEndPoint);
		}

		public void Send(OscPacket packet)
		{
			byte[] data = packet.GetBytes();
			Send(data);
		}

		bool closing = false;
		public void Close()
		{
			lock (callbackLock)
			{
				ClosingEvent.Reset();
				closing = true;
				proxyUdpClient.Close();
			}
			ClosingEvent.WaitOne();
			
		}

		public void Dispose()
		{
			this.Close();
		}

		public OscPacket Receive()
		{
			if (closing) throw new Exception("UDPListener has been closed.");

			lock (queue)
			{
				if (queue.Count() > 0)
				{
					byte[] bytes = queue.Dequeue();
					var packet = OscPacket.GetPacket(bytes);
					return packet;
				}
				else
					return null;
			}
		}

		public byte[] ReceiveBytes()
		{
			if (closing) throw new Exception("UDPListener has been closed.");

			lock (queue)
			{
				if (queue.Count() > 0)
				{
					byte[] bytes = queue.Dequeue();
					return bytes;
				}
				else
					return null;
			}
		}
		
	}
}
