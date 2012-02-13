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
		public int Port { get; private set; }
		
		object callbackLock;

		UdpClient receivingUdpClient;
		IPEndPoint RemoteIpEndPoint;

		HandleBytePacket BytePacketCallback = null;
		HandleOscPacket OscPacketCallback = null;

		Queue<byte[]> queue;
		ManualResetEvent ClosingEvent;

		public UDPListener(int port)
		{
			Port = port;
			queue = new Queue<byte[]>();
			ClosingEvent = new ManualResetEvent(false);
			callbackLock = new object();

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

			// setup first async event
			AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
			receivingUdpClient.BeginReceive(callBack, null);
		}

		public UDPListener(int port, HandleOscPacket callback) : this(port)
		{
			this.OscPacketCallback = callback;
		}

		public UDPListener(int port, HandleBytePacket callback) : this(port)
		{
			this.BytePacketCallback = callback;
		}

		void ReceiveCallback(IAsyncResult result)
		{
			Monitor.Enter(callbackLock);
			Byte[] bytes = null;

			try
			{
				bytes = receivingUdpClient.EndReceive(result, ref RemoteIpEndPoint);
			}
			catch (ObjectDisposedException e)
			{ 
				// Ignore if disposed. This happens when closing the listener
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
				receivingUdpClient.BeginReceive(callBack, null);
			}
			Monitor.Exit(callbackLock);
		}

		bool closing = false;
		public void Close()
		{
			lock (callbackLock)
			{
				ClosingEvent.Reset();
				closing = true;
				receivingUdpClient.Close();
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
