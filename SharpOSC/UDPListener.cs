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
		
		object listenerLock;

		UdpClient receivingUdpClient;
		IPEndPoint RemoteIpEndPoint;

		private HandleBytePacket BytePacketCallback = null;
		private HandleOscPacket OscPacketCallback = null;

		private Queue<byte[]> queue;

		public UDPListener(int port)
		{
			listenerLock = new object(); 
			_port = port;
			queue = new Queue<byte[]>();

			// try to open the port 10 times, else fail
			for (int i = 0; i < 10; i++)
			{
				try
				{
					receivingUdpClient = new UdpClient(port);
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
			lock (listenerLock)
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
			}
		}

		bool closed = false;
		public void Close()
		{
			closed = true;
			receivingUdpClient.Close();
			
			// Wait for the lock to become open, then we know the listener has stopped
			lock (listenerLock) { }
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
