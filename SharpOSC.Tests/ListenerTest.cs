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
    }
}
