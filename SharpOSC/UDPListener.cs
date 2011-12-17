using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SharpOSC
{
	public delegate void HandleOscPacket(OscPacket packet);
	public delegate void HandleBytePacket(byte[] packet);

	public class UDPListener : IDisposable
	{
		public int Port
		{
			get { return this._port; }
		}
		int _port;
		Thread thread;
		UdpClient receivingUdpClient;
		IPEndPoint RemoteIpEndPoint;

		private HandleBytePacket BytePacketCallback = null;
		private HandleOscPacket OscPacketCallback = null;

		private Queue<byte[]> queue;

		public UDPListener(int port)
		{
			_port = port;
			queue = new Queue<byte[]>();

			receivingUdpClient = new UdpClient(port);
			RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
			thread = new Thread(new ThreadStart(Listen));
			thread.Start();
		}

		public UDPListener(int port, HandleOscPacket callback) : this(port)
		{
			this.OscPacketCallback = callback;
		}

		public UDPListener(int port, HandleBytePacket callback) : this(port)
		{
			this.BytePacketCallback = callback;
		}

		public void Listen()
		{
			lock (thread)
			{
				while (!closed)
				{
					Byte[] bytes = null;
					try
					{
						bytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
					}
					catch (System.Net.Sockets.SocketException e)
					{
						if (closed)
							break;	// Shit breaks when I'm closing the socket, just break the loop
									// Todo: Fix this shit
						else
							throw e;
					}

					if (bytes == null || bytes.Length == 0)
						continue;

					if (BytePacketCallback != null)
					{
						BytePacketCallback(bytes);
					}
					else if (OscPacketCallback != null)
					{
						var packet = OscPacket.GetPacket(bytes);
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
			}
		}

		bool closed = false;
		public void Close()
		{
			closed = true;
			receivingUdpClient.Close();
			lock (thread)
			{
				thread.Abort(); 
			}
		}

		public void Dispose()
		{
			this.Close();
		}

		public OscPacket Receive()
		{
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
