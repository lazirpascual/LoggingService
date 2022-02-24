/*
FILE                : TestingClient.py
PROJECT			    : A3 - Network Application Development
PROGRAMMER		    : Lazir Pascual, Rohullah Noory
FIRST VERSION       : 2/15/2022
DESCRIPTION		    :
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Collections.Specialized;
using System.Web;
using System.Net.NetworkInformation;


// The following code is extracted from the MSDN site:
//https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=net-5.0
//

namespace LoggingService
{
    class Program
    {
        static LoggingEngine logEngine = new LoggingEngine();
        
        static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                Int32 port = Int32.Parse(ConfigurationManager.AppSettings.Get("port"));
                string ipAddress = ConfigurationManager.AppSettings.Get("ipAddress"); // read minValue from config file
                IPAddress localAddr = IPAddress.Parse(ipAddress);
                server = new TcpListener(localAddr, port);
                server.Start(); // Start listening for client requests.
                Console.Write("Server is currently running.\n");
                Console.Write("Waiting for a connection... \n\n");
                // Enter the listening loop.
                while (true)
                {
                    // Perform a blocking call to accept requests.
                    TcpClient client = server.AcceptTcpClient();
                    ParameterizedThreadStart ts = new ParameterizedThreadStart(Worker);
                    Thread clientThread = new Thread(ts);
                    clientThread.Start(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        /*  -- Function Header
            Name	:	Worker()
            Purpose :	This method is passed to a thread as a parameter, which executes when the
                        server receives and accepts a connection from the Client socket. It contains
                        the different responses to the requests sent from the client. This method
                        also contains the main game logic of the Hi-Lo game, where the min, max,
                        random values are determined and the state is managed.
            Inputs	:	Object o - the TCP client object that the server is currently listening to
            Returns	:	NONE
        */
        public static void Worker(Object o)
        {
            TcpClient client = (TcpClient)o;
            // Buffer for reading data
            String response = null;
            String request = null;
            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            request = ReceiveRequest(stream);
            response = logEngine.ProcessRequest(request);
            SendResponse(stream, response);

            client.Close(); // Shutdown and end connection
        }

        /*  -- Function Header
            Name	:	ReceiveRequest()
            Purpose :	This method reads from an Network Stream, receiving bytes
                        of data from the client containing the client's request.
            Inputs	:	NetworkStream stream - Current Network Stream that is used to receive data to the client
            Returns	:	NONE
        */
        public static string ReceiveRequest(NetworkStream stream)
        {
            Byte[] bytes = new Byte[256];
            int i = stream.Read(bytes, 0, bytes.Length);
            string request = Encoding.ASCII.GetString(bytes, 0, i); // Translate data bytes to a ASCII string.    
            return request;
        }

        /*  -- Function Header
            Name	:	SendResponse()
            Purpose :	This method writes to the existing Network Stream, sending bytes
                        of data to the client containing the server's reponse.
            Inputs	:	NetworkStream stream - Current Network Stream that is used to send data to the client
                        string data - message that is being sent to the client
            Returns	:	NONE
        */
        public static void SendResponse(NetworkStream stream, string response)
        {
            byte[] msg = Encoding.ASCII.GetBytes(response);
            stream.Write(msg, 0, msg.Length);   // Send back a response.
        }
    }
}
