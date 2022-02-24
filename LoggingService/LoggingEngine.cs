using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web;
using System.Configuration;

namespace LoggingService
{
    class LoggingEngine
    {
        /*  -- Function Header
            Name	:	ProcessRequest()
            Purpose :	This method is is used to process the request that the client is sending to the server.
                        Depending on the request type, it will perform the game logic accordingly and sends back
                        a response.
            Inputs	:	string queryString - query string that is being processed (sent from the client)
            Returns	:	response being sent back to the client
        */
        public string ProcessRequest(string queryString)
        {
            NameValueCollection qsCollection = HttpUtility.ParseQueryString(queryString);  // parse query string sent from client
            string requestType = qsCollection["request"];   // parse request type of the query string           
            string response = null;

            switch (requestType)
            {
                case "TEST1":
                    response = "Hi python! I am C#";
                    break;
                default:
                    response = "Command unknown";
                    break;
            }

            DisplayOutput(queryString, response, requestType);
            return response;
        }

        /*  -- Function Header
            Name	:	DisplayOutput()
            Purpose :	This method contains the logic of displaying output when a client makes a 
                        connects with a server. The request type, data being received and data being
                        sent back are all displayed on the console.
            Inputs	:	string request - string that contains data being received from client
                        string response - string that is being sent back to the client
                        string responseType - type of request currently being processed
            Returns	:	NONE
        */
        public void DisplayOutput(string request, string response, string requestType)
        {
            Console.WriteLine($"A client has connected with a {requestType} request.");
            Console.WriteLine("Received: {0}", request);
            Console.WriteLine("Sent: {0}\n", response);
        }
    }
}
