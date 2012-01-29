using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;

namespace SharpOSC.Tests
{
	[TestFixture]
	class ListenerTest
	{

		/// <summary>
		/// Opens a listener on a specified port, then closes it and attempts to open another on the same port
		/// Opening the second listener will fail unless the first one has been properly closed.
		/// </summary>
		[TestCase]
		public void CloseListener()
		{
			var l1 = new UDPListener(55555);
			var isnull = l1.Receive();
			l1.Close();

			var l2 = new UDPListener(55555);
			isnull = l2.Receive();
			l2.Close();
		}

		/// <summary>
		/// Tries to open two listeners on the same port, results in an exception
		/// </summary>
		[TestCase]
		public void CloseListenerException()
		{
			UDPListener l1 = null;
			bool ex = false;
			try
			{
				l1 = new UDPListener(55555);
				var isnull = l1.Receive();
				var l2 = new UDPListener(55555);
			}
			catch (Exception e)
			{
				ex = true;
			}

			Assert.IsTrue(ex);
			l1.Close();
		}

		/// <summary>
		/// Bombard the listener with messages, check if they are all received
		/// </summary>
		[TestCase]
		public void ListenerLoadTest()
		{
			var listener = new UDPListener(55555);

			var sender = new SharpOSC.UDPSender("localhost", 55555);

			var msg = new SharpOSC.OscMessage("/test/", 23.42f);

			for (int i = 0; i < 1000; i++)
				sender.Send(msg);

			for (int i = 0; i < 1000; i++)
			{
				var receivedMessage = listener.Receive();
				Assert.NotNull(receivedMessage);
			}

			listener.Dispose();
		}
	}
}
