SharpOSC - OSC Library for .NET 3.5
===================================


SharpOSC is a small library designed to make interacting with Open Sound Control easy (OSC). It provides the following features:

+ Produce an OSC Packet (messages and bundles) from .NET values. 
+ Translate an OSC message (consisting of a sequence of bytes) into an easy-to-use .NET object.
+ Transmit OSC packets via UDP.
+ Receive OSC packets via UDP.

Currently all standard OSC types are supported as well as the "nonstandard" types (this includes arrays, but currently nested arrays are not supported. The OSC definition is unclear about whether nested arrays are even allowed)

Download
--------

If you don't want to build SharpOSC from source you can download the compiled dll assembly by clicking "Download" in the tab menu)

The latest version (version 0.1.0.0) can be found [Here](https://github.com/downloads/valdiorn/SharpOSC/SharpOSC_v0.1.0.0.zip)


License
-------

SharpOSC is licensed under the MIT license. 

See License.txt

Using The Library
-----------------

To use the library add a reference to SharpOSC.dll in your .NET project. SharpOSC should now be available to use in your code under that namespace "SharpOSC". 

Example: Sending a message
--------------------------

	class Program
	{
		static void Main(string[] args)
		{
			var message = new SharpOSC.OscMessage("/test/1", 23, 42.01f, "hello world");
			var sender = new SharpOSC.UDPSender("127.0.0.1", 55555);
			sender.Send(message);
		}
	}

This example sends an OSC message to the local machine on port 55555 containing 3 arguments: an integer with a value of 23, a floating point number with the value 42.01 and the string "hello world". If another program is listening to port 55555 it will receive the message and be able to use the data sent.

Example: Receiving a Message (Synchronous)
------------------------------------------

	class Program
	{
		static void Main(string[] args)
		{
			var listener = new UDPListener(55555);
			OscMessage messageReceived = null;
			while (messageReceived == null)
			{
				messageReceived = (OscMessage)listener.Receive();
				Thread.Sleep(1);
			}
		}
	}

This shows a very simple way of waiting for incoming messages. The listener.Receive() method will check if the listener has received any new messages since it was last called. If there is a new message that has not been returned it will assign messageReceived to point to that message. If no message has been received since the last call to Receive it will return null.

Example: Receiving a Message (Asynchronous)
-------------------------------------------

	class Program
	{
		public void Main(string[] args)
		{
			// The cabllback function
			HandleOscPacket callback = delegate(OscPacket packet)
			{
				var messageReceived = (OscMessage)packet;
			};

			var listener = new UDPListener(55555, callback);

			Console.WriteLine("Press enter to stop");
			listener.Close();
		}
	}

By giving UDPListener a callback you don't have to periodically check for incoming messages. The listener will simply invoke the callback whenever a message is received. You are free to implement any code you need inside the callback.