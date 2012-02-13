SharpOSC - OSC Library for .NET 3.5
===================================


SharpOSC is a small library designed to make interacting with Open Sound Control easy (OSC). It provides the following features:

+ Produce an OSC Packet (messages and bundles) from .NET values.
+ Translate an OSC message (consisting of a sequence of bytes) into a .NET object.
+ Transmit OSC packets via UDP.
+ Receive OSC packets via UDP.

Download
--------

If you don't want to build SharpOSC from source you can download compiled versions from [Here](https://github.com/valdiorn/SharpOSC/tree/master/Binaries)


Supported Types
---------------

[The following OSC types](http://opensoundcontrol.org/spec-1_0) are supported:

* i	- int32 (System.Int32)
* f	- float32 (System.Single)
* s	- OSC-string (System.String)
* b	- OSC-blob (System.Byte[])
* h	- 64 bit big-endian two's complement integer (System.Int64)
* t	- OSC-timetag (System.UInt64 / SharpOSC.Timetag)
* d	- 64 bit ("double") IEEE 754 floating point number (System.Double)
* S	- Alternate type represented as an OSC-string (for example, for systems that differentiate "symbols" from "strings") (SharpOSC.Symbol)
* c	- an ascii character, sent as 32 bits (System.Char)
* r	- 32 bit RGBA color (SharpOSC.RGBA)
* m	- 4 byte MIDI message. Bytes from MSB to LSB are: port id, status byte, data1, data2 (SharpOSC.Midi)
* T	- True. No bytes are allocated in the argument data. (System.Boolean)
* F	- False. No bytes are allocated in the argument data. (System.Boolean)
* N	- Nil. No bytes are allocated in the argument data. (null)
* I	- Infinitum. No bytes are allocated in the argument data. (Double.PositiveInfinity)
* [	- Indicates the beginning of an array. The tags following are for data in the Array until a close brace tag is reached. (System.Object[] / List\<object\>)
* ]	- Indicates the end of an array.

(Note that nested arrays (arrays within arrays) are not supported, the OSC specification is unclear about whether that it is even allowed)



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
			Console.WriteLine("Received a message!");
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
				Console.WriteLine("Received a message!");
			};

			var listener = new UDPListener(55555, callback);

			Console.WriteLine("Press enter to stop");
			listener.Close();
		}
	}

By giving UDPListener a callback you don't have to periodically check for incoming messages. The listener will simply invoke the callback whenever a message is received. You are free to implement any code you need inside the callback.

Contribute
----------

I would love to get some feedback. Use the Issue tracker on Github to send bug reports and feature requests, or just if you have something to say about the project. If you have code changes that you would like to have integrated into the main repository, send me a pull request or a patch. I will try my best to integrate them and make sure SharpOSC improves and matures.