using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpOSC.Tests
{
	class Program
	{
		static void Main(string[] args)
		{
			var message = new SharpOSC.OscMessage("/test/1", 23, 42.01f, "hello world");
			var sender = new SharpOSC.UDPSender("127.0.0.1", 55555);
			sender.Send(message);
			var listener = new UDPListener(55555);
			OscMessage messageReceived = null;
			while (messageReceived == null)
			{
				messageReceived = (OscMessage)listener.Receive();
				Thread.Sleep(1);
			}
		}

		static void Main2()
		{
			var listener = new UDPListener(55555);
			var messageReceived = (OscMessage)listener.Receive();
		}
	}
}
