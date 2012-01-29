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
			/*var message = new SharpOSC.OscMessage("/Knob", 0.5f);
			var sender = new SharpOSC.UDPSender("127.0.0.1", 10000);
			
			while (true)
			{
				var inp = Console.ReadLine();
				float f = Convert.ToSingle(inp);
				message.Arguments[0] = f;
				sender.Send(message);
			}*/

			HandleOscPacket cb = delegate(OscPacket packet)
			{
				var msg = ((OscBundle)packet).Messages[0];
				Console.WriteLine(msg.Arguments[0].ToString());
			};

			var l1 = new UDPListener(10001, cb);

			Console.ReadLine();
		}

	}
}
